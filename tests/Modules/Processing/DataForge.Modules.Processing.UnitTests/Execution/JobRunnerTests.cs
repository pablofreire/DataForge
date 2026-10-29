using System.Diagnostics.Metrics;
using System.Reflection;
using DataForge.BuildingBlocks.Application.Tenancy;
using DataForge.Modules.Processing.Application.Diagnostics;
using DataForge.Modules.Processing.Application.Execution;
using DataForge.Modules.Processing.Application.Options;
using DataForge.Modules.Processing.Contracts.Exceptions;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Models;
using DataForge.Modules.Processing.Domain.Entities;
using DataForge.Modules.Processing.Domain.Enums;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DataForge.Modules.Processing.UnitTests.Execution;

public sealed class JobRunnerTests
{
    private const string WorkerId = "worker-1";
    private const string JobType = "test.job";

    private readonly IJobRepository _repository = Substitute.For<IJobRepository>();
    private readonly TenantContext _tenantContext = new();

    [Fact]
    public async Task RunAsync_ExecutorSucceeds_MarksJobSucceededForThisWorker()
    {
        var job = ClaimedJob(attempt: 1);
        var executor = new DelegateExecutor((_, _) => Task.CompletedTask);

        await CreateRunner(executor).RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        await _repository.Received(1).MarkSucceededAsync(job.Id, WorkerId, Arg.Any<CancellationToken>());
        _tenantContext.TenantId.Should().Be(job.TenantId, "the executor must run in the job's tenant");
    }

    [Fact]
    public async Task RunAsync_TransientFailureWithAttemptsLeft_SchedulesRetryWithoutLeakingInternals()
    {
        var job = ClaimedJob(attempt: 1);
        var executor = new DelegateExecutor((_, _) => throw new TimeoutException("connection string Password=secret"));

        await CreateRunner(executor).RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        await _repository.Received(1).ScheduleRetryAsync(
            job.Id,
            WorkerId,
            Arg.Any<TimeSpan>(),
            Arg.Is<string>(message => !message.Contains("secret", StringComparison.Ordinal) && message.Contains("TimeoutException", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
        await _repository.DidNotReceiveWithAnyArgs().MarkFailedAsync(default, default!, default!, default);
    }

    [Fact]
    public async Task RunAsync_TransientFailureOnLastAttempt_MarksFailed()
    {
        var job = ClaimedJob(attempt: 3);
        var executor = new DelegateExecutor((_, _) => throw new IOException("disk"));

        await CreateRunner(executor).RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        await _repository.Received(1).MarkFailedAsync(job.Id, WorkerId, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceiveWithAnyArgs().ScheduleRetryAsync(default, default!, default, default!, default);
    }

    [Fact]
    public async Task RunAsync_NonRetryableFailure_FailsImmediatelyWithExecutorMessage()
    {
        var job = ClaimedJob(attempt: 1);
        var executor = new DelegateExecutor((_, _) => throw new NonRetryableJobException("The file has no header row."));

        await CreateRunner(executor).RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        await _repository.Received(1).MarkFailedAsync(job.Id, WorkerId, "The file has no header row.", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ResumedJob_ExecutorReceivesCheckpoint()
    {
        var job = ClaimedJob(attempt: 2, checkpoint: 15_000);
        JobExecutionContext? received = null;
        var executor = new DelegateExecutor((context, _) =>
        {
            received = context;
            return Task.CompletedTask;
        });

        await CreateRunner(executor).RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        received!.CheckpointPosition.Should().Be(15_000);
        received.Attempt.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_ProgressRejectedBecauseLeaseWasLost_DoesNotRecordAnyOutcome()
    {
        var job = ClaimedJob(attempt: 1);
        _repository.SaveProgressAsync(default, default!, default, default, default, default).ReturnsForAnyArgs(false);
        var executor = new DelegateExecutor((context, token) => context.Progress.ReportAsync(10, null, 10, token));

        await CreateRunner(executor).RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        await _repository.DidNotReceiveWithAnyArgs().MarkSucceededAsync(default, default!, default);
        await _repository.DidNotReceiveWithAnyArgs().MarkFailedAsync(default, default!, default!, default);
        await _repository.DidNotReceiveWithAnyArgs().ScheduleRetryAsync(default, default!, default, default!, default);
    }

    [Fact]
    public async Task RunAsync_NoExecutorForType_MarksFailed()
    {
        var job = ClaimedJob(attempt: 1);

        await CreateRunner().RunAsync(job, WorkerId, TestContext.Current.CancellationToken);

        await _repository.Received(1).MarkFailedAsync(job.Id, WorkerId, Arg.Is<string>(message => message.Contains(JobType, StringComparison.Ordinal)), Arg.Any<CancellationToken>());
    }

    private JobRunner CreateRunner(params IJobExecutor[] executors) =>
        new(
            _repository,
            executors,
            _tenantContext,
            new ProcessingMetrics(new TestMeterFactory()),
            Microsoft.Extensions.Options.Options.Create(new ProcessingOptions { MaxAttempts = 3, LeaseDuration = TimeSpan.FromMinutes(1) }),
            TimeProvider.System,
            NullLogger<JobRunner>.Instance);

    /// <summary>A job as returned by the claim query: running, with its attempt counter already incremented.</summary>
    private static JobEntity ClaimedJob(int attempt, long checkpoint = 0)
    {
        var job = JobEntity.Create(42, Guid.NewGuid(), JobType, Guid.NewGuid(), Guid.NewGuid().ToString(), maxAttempts: 3, DateTimeOffset.UtcNow);
        Set(job, nameof(JobEntity.Id), 1001L);
        Set(job, nameof(JobEntity.Attempt), attempt);
        Set(job, nameof(JobEntity.CheckpointPosition), checkpoint);
        Set(job, nameof(JobEntity.Status), JobStatus.Running);
        return job;
    }

    private static void Set<T>(JobEntity job, string property, T value) =>
        typeof(JobEntity).GetProperty(property, BindingFlags.Instance | BindingFlags.Public)!.SetValue(job, value);

    private sealed class DelegateExecutor(Func<JobExecutionContext, CancellationToken, Task> execute) : IJobExecutor
    {
        public string JobType => JobRunnerTests.JobType;

        public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken) => execute(context, cancellationToken);
    }

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);

        public void Dispose()
        {
        }
    }
}
