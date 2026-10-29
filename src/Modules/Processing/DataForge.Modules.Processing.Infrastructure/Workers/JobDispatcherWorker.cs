using DataForge.Modules.Processing.Application.Execution;
using DataForge.Modules.Processing.Application.Options;
using DataForge.Modules.Processing.Domain.Entities;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Processing.Infrastructure.Workers;

/// <summary>
/// Polls the durable queue and runs up to <see cref="ProcessingOptions.MaxConcurrentJobs"/> jobs in parallel,
/// each in its own DI scope. On shutdown it stops claiming and lets running jobs release themselves so
/// another worker can resume them from their checkpoints.
/// </summary>
public sealed partial class JobDispatcherWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ProcessingOptions> options,
    TimeProvider timeProvider,
    ILogger<JobDispatcherWorker> logger) : BackgroundService
{
    private static readonly TimeSpan MaintenanceInterval = TimeSpan.FromMinutes(1);

    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        using var slots = new SemaphoreSlim(settings.MaxConcurrentJobs, settings.MaxConcurrentJobs);
        var running = new List<Task>();
        var nextMaintenance = timeProvider.GetUtcNow();

        LogStarted(logger, _workerId, settings.MaxConcurrentJobs);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await slots.WaitAsync(stoppingToken);
                running.RemoveAll(task => task.IsCompleted);

                if (timeProvider.GetUtcNow() >= nextMaintenance)
                {
                    await RunMaintenanceAsync(stoppingToken);
                    nextMaintenance = timeProvider.GetUtcNow().Add(MaintenanceInterval);
                }

                var claimed = await TryClaimAsync(settings, stoppingToken);
                if (claimed is null)
                {
                    slots.Release();
                    await Task.Delay(settings.PollInterval, timeProvider, stoppingToken);
                    continue;
                }

                running.Add(RunJobAsync(claimed, slots, stoppingToken));
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }

        await Task.WhenAll(running);
        LogStopped(logger, _workerId);
    }

    private async Task<JobEntity?> TryClaimAsync(ProcessingOptions settings, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            return await repository.TryClaimNextAsync(_workerId, settings.LeaseDuration, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
#pragma warning disable CA1031 // The dispatcher survives transient database outages and retries on the next poll.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            LogClaimFailed(logger, exception);
            return null;
        }
    }

    private async Task RunJobAsync(JobEntity job, SemaphoreSlim slots, CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredService<JobRunner>();
            await runner.RunAsync(job, _workerId, stoppingToken);
        }
#pragma warning disable CA1031 // A failure while recording the outcome must not crash the worker; the lease will expire and the job is retried.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            LogRunFailed(logger, exception, job.PublicId);
        }
        finally
        {
            slots.Release();
        }
    }

    private async Task RunMaintenanceAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var failed = await repository.FailAbandonedAsync(stoppingToken);
            if (failed > 0)
            {
                LogAbandonedFailed(logger, failed);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
#pragma warning disable CA1031 // Maintenance is best-effort.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            LogClaimFailed(logger, exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Job dispatcher {WorkerId} started with {MaxConcurrentJobs} slots")]
    private static partial void LogStarted(ILogger logger, string workerId, int maxConcurrentJobs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Job dispatcher {WorkerId} stopped")]
    private static partial void LogStopped(ILogger logger, string workerId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to poll the job queue; retrying")]
    private static partial void LogClaimFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Job {JobId} could not be completed by the runner")]
    private static partial void LogRunFailed(ILogger logger, Exception exception, Guid jobId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Marked {Count} abandoned jobs as failed")]
    private static partial void LogAbandonedFailed(ILogger logger, int count);
}
