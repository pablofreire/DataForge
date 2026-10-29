using DataForge.BuildingBlocks.Application.Tenancy;
using DataForge.Modules.Processing.Application.Diagnostics;
using DataForge.Modules.Processing.Application.Exceptions;
using DataForge.Modules.Processing.Application.Models;
using DataForge.Modules.Processing.Application.Options;
using DataForge.Modules.Processing.Contracts.Exceptions;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Models;
using DataForge.Modules.Processing.Domain.Entities;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using DataForge.Modules.Processing.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Processing.Application.Execution;

/// <summary>
/// Runs one claimed job attempt: binds the tenant, resolves the executor, keeps the lease alive with a
/// heartbeat, enforces the timeout and records the outcome (succeeded, retry scheduled, failed,
/// cancelled, or released on shutdown).
/// </summary>
public sealed partial class JobRunner(
    IJobRepository jobRepository,
    IEnumerable<IJobExecutor> executors,
    TenantContext tenantContext,
    ProcessingMetrics metrics,
    IOptions<ProcessingOptions> options,
    TimeProvider timeProvider,
    ILogger<JobRunner> logger)
{
    public async Task RunAsync(JobEntity job, string workerId, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);

        tenantContext.SetTenant(job.TenantId, job.TenantPublicId);

        var executor = executors.FirstOrDefault(candidate => string.Equals(candidate.JobType, job.JobType, StringComparison.Ordinal));
        if (executor is null)
        {
            LogNoExecutor(logger, job.JobType, job.PublicId);
            await jobRepository.MarkFailedAsync(job.Id, workerId, $"No executor is registered for job type '{job.JobType}'.", CancellationToken.None);
            return;
        }

        var settings = options.Value;
        var state = new JobAttemptState();
        using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        attemptCts.CancelAfter(settings.JobTimeout);
        using var heartbeatCts = new CancellationTokenSource();
        var heartbeat = RunHeartbeatAsync(job.Id, workerId, state, attemptCts, heartbeatCts.Token);

        var startedAt = timeProvider.GetTimestamp();
        var outcome = "unknown";
        metrics.JobStarted(job.JobType);
        LogStarted(logger, job.JobType, job.PublicId, job.Attempt, job.CheckpointPosition);

        try
        {
            var progress = new JobProgressReporter(jobRepository, metrics, job.Id, job.JobType, workerId, job.ProcessedItems);
            var context = new JobExecutionContext(job.PublicId, job.ResourceId, job.Attempt, job.CheckpointPosition, progress);

            await executor.ExecuteAsync(context, attemptCts.Token);
            await jobRepository.MarkSucceededAsync(job.Id, workerId, CancellationToken.None);
            outcome = "succeeded";
        }
        catch (Exception exception) when (state.LeaseLost || exception is JobLeaseLostException)
        {
            // Another worker owns the job now; it will finish the work from the last checkpoint.
            outcome = "lease_lost";
            LogLeaseLost(logger, job.PublicId);
        }
        catch (OperationCanceledException) when (state.CancelRequested)
        {
            await jobRepository.MarkCancelledAsync(job.Id, workerId, CancellationToken.None);
            outcome = "cancelled";
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            await jobRepository.ReleaseAsync(job.Id, workerId, CancellationToken.None);
            outcome = "released";
        }
        catch (NonRetryableJobException exception)
        {
            await jobRepository.MarkFailedAsync(job.Id, workerId, exception.Message, CancellationToken.None);
            outcome = "failed";
            LogFailed(logger, exception, job.PublicId);
        }
#pragma warning disable CA1031 // Any other failure is recorded on the job and retried; the worker must keep running.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            // Unexpected exception messages may contain internals, so users only see a generic description.
            var message = exception is OperationCanceledException
                ? $"The attempt timed out after {settings.JobTimeout}."
                : $"Unexpected processing error ({exception.GetType().Name}).";

            if (job.HasAttemptsLeft)
            {
                var delay = JobRetryPolicy.ComputeDelay(job.Attempt, settings.RetryBaseDelay, settings.RetryMaxDelay);
                await jobRepository.ScheduleRetryAsync(job.Id, workerId, delay, message, CancellationToken.None);
                outcome = "retry_scheduled";
            }
            else
            {
                await jobRepository.MarkFailedAsync(job.Id, workerId, message, CancellationToken.None);
                outcome = "failed";
            }

            LogFailed(logger, exception, job.PublicId);
        }
        finally
        {
            await heartbeatCts.CancelAsync();
            await heartbeat;
            metrics.JobFinished(job.JobType, outcome, timeProvider.GetElapsedTime(startedAt));
            LogFinished(logger, job.JobType, job.PublicId, outcome);
        }
    }

    private async Task RunHeartbeatAsync(long jobId, string workerId, JobAttemptState state, CancellationTokenSource attemptCts, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var timer = new PeriodicTimer(settings.HeartbeatInterval, timeProvider);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    var lease = await jobRepository.RenewLeaseAsync(jobId, workerId, settings.LeaseDuration, cancellationToken);
                    if (!lease.IsOwned)
                    {
                        state.MarkLeaseLost();
                        await attemptCts.CancelAsync();
                        return;
                    }

                    if (lease.CancelRequested)
                    {
                        state.MarkCancelRequested();
                        await attemptCts.CancelAsync();
                        return;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
#pragma warning disable CA1031 // A transient failure to renew is retried on the next tick; the lease outlives several ticks.
                catch (Exception exception)
#pragma warning restore CA1031
                {
                    LogHeartbeatFailed(logger, exception, jobId);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The attempt finished; stop renewing.
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "No executor registered for job type {JobType} (job {JobId})")]
    private static partial void LogNoExecutor(ILogger logger, string jobType, Guid jobId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting {JobType} job {JobId}, attempt {Attempt}, checkpoint {Checkpoint}")]
    private static partial void LogStarted(ILogger logger, string jobType, Guid jobId, int attempt, long checkpoint);

    [LoggerMessage(Level = LogLevel.Information, Message = "Finished {JobType} job {JobId} with outcome {Outcome}")]
    private static partial void LogFinished(ILogger logger, string jobType, Guid jobId, string outcome);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Lease of job {JobId} was lost to another worker")]
    private static partial void LogLeaseLost(ILogger logger, Guid jobId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Job {JobId} attempt failed")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid jobId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Heartbeat for job {JobId} failed; will retry on the next tick")]
    private static partial void LogHeartbeatFailed(ILogger logger, Exception exception, long jobId);
}
