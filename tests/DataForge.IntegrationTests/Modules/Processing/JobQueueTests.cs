using System.Collections.Concurrent;
using DataForge.IntegrationTests.Fixtures;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.IntegrationTests.Modules.Processing;

public sealed class JobQueueTests(PostgresFixture database)
{
    private const string TestJobType = "test.noop";

    [Fact]
    public async Task ConcurrentWorkers_EachJobIsClaimedExactlyOnce()
    {
        const int jobCount = 40;
        await using var services = database.BuildServices();
        await EnqueueAsync(services, jobCount);
        var repository = services.GetRequiredService<IJobRepository>();
        var claimed = new ConcurrentBag<(long JobId, string Worker)>();

        await Parallel.ForEachAsync(Enumerable.Range(1, 8), async (worker, token) =>
        {
            while (await repository.TryClaimNextAsync($"worker-{worker}", TimeSpan.FromMinutes(1), token) is { } job)
            {
                claimed.Add((job.Id, $"worker-{worker}"));
            }
        });

        claimed.Should().HaveCount(jobCount);
        claimed.Select(claim => claim.JobId).Should().OnlyHaveUniqueItems("FOR UPDATE SKIP LOCKED never hands the same job to two workers");
        claimed.Select(claim => claim.Worker).Distinct().Count().Should().BeGreaterThan(1);

        foreach (var (jobId, worker) in claimed)
        {
            await repository.MarkSucceededAsync(jobId, worker, CancellationToken.None);
        }
    }

    [Fact]
    public async Task ExpiredLease_IsTakenOverAndTheOldOwnerIsFenced()
    {
        await using var services = database.BuildServices();
        await EnqueueAsync(services, 1);
        var repository = services.GetRequiredService<IJobRepository>();

        var first = await repository.TryClaimNextAsync("worker-a", TimeSpan.FromMilliseconds(500), CancellationToken.None);
        first.Should().NotBeNull();
        await Task.Delay(TimeSpan.FromSeconds(1.2), TestContext.Current.CancellationToken);

        var second = await repository.TryClaimNextAsync("worker-b", TimeSpan.FromMinutes(1), CancellationToken.None);

        second!.Id.Should().Be(first!.Id, "a job whose worker stopped renewing its lease is resumed by another worker");
        second.Attempt.Should().Be(2);
        (await repository.SaveProgressAsync(first.Id, "worker-a", 10, null, 10, CancellationToken.None)).Should().BeFalse("the old owner is fenced out");
        (await repository.SaveProgressAsync(first.Id, "worker-b", 10, null, 10, CancellationToken.None)).Should().BeTrue();
        (await repository.RenewLeaseAsync(first.Id, "worker-a", TimeSpan.FromMinutes(1), CancellationToken.None)).IsOwned.Should().BeFalse();

        await repository.MarkSucceededAsync(first.Id, "worker-b", CancellationToken.None);
    }

    [Fact]
    public async Task Enqueue_SameIdempotencyKey_ReturnsTheSameJob()
    {
        await using var services = database.BuildServices();
        await using var scope = await database.CreateTenantScopeAsync(services, "acme");
        var scheduler = scope.ServiceProvider.GetRequiredService<IJobScheduler>();
        var key = $"test:{Guid.NewGuid():N}";
        var resource = Guid.NewGuid();

        var first = await scheduler.EnqueueAsync(TestJobType, resource, key, CancellationToken.None);
        var second = await scheduler.EnqueueAsync(TestJobType, resource, key, CancellationToken.None);

        second.JobId.Should().Be(first.JobId);

        var job = await scope.ServiceProvider.GetRequiredService<IJobRepository>().TryClaimNextAsync("cleanup", TimeSpan.FromMinutes(1), CancellationToken.None);
        await scope.ServiceProvider.GetRequiredService<IJobRepository>().MarkSucceededAsync(job!.Id, "cleanup", CancellationToken.None);
    }

    private async Task EnqueueAsync(ServiceProvider services, int count)
    {
        await using var scope = await database.CreateTenantScopeAsync(services, "acme");
        var scheduler = scope.ServiceProvider.GetRequiredService<IJobScheduler>();
        for (var index = 0; index < count; index++)
        {
            await scheduler.EnqueueAsync(TestJobType, Guid.NewGuid(), $"test:{Guid.NewGuid():N}", CancellationToken.None);
        }
    }
}
