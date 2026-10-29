using DataForge.Modules.Processing.Application.Execution;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.IntegrationTests.Fixtures;

/// <summary>
/// Drives the real queue deterministically: claims with the production SQL and runs each job with the
/// production <see cref="JobRunner"/>, exactly as the Worker's dispatcher does, until the queue is empty.
/// </summary>
internal static class JobDriver
{
    public static async Task<int> RunUntilIdleAsync(IServiceProvider services, string workerId = "integration-worker", int maxJobs = 100)
    {
        var processed = 0;
        while (processed < maxJobs)
        {
            await using var scope = services.CreateAsyncScope();
            var job = await scope.ServiceProvider.GetRequiredService<IJobRepository>()
                .TryClaimNextAsync(workerId, TimeSpan.FromMinutes(1), CancellationToken.None);
            if (job is null)
            {
                return processed;
            }

            await scope.ServiceProvider.GetRequiredService<JobRunner>().RunAsync(job, workerId, CancellationToken.None);
            processed++;
        }

        throw new InvalidOperationException($"The queue did not drain after {maxJobs} jobs.");
    }
}
