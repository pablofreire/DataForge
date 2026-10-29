using DataForge.Modules.Processing.Application.Diagnostics;
using DataForge.Modules.Processing.Application.Exceptions;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Processing.Application.Execution;

/// <summary>
/// Persists progress for one running job attempt. Writes are fenced by the worker id, so progress from a
/// worker that lost its lease is rejected and that worker is stopped.
/// </summary>
public sealed class JobProgressReporter(
    IJobRepository jobRepository,
    ProcessingMetrics metrics,
    long jobId,
    string jobType,
    string workerId,
    long initialProcessedItems) : IJobProgressReporter
{
    private long _lastProcessedItems = initialProcessedItems;

    public async Task ReportAsync(long processedItems, long? totalItems, long checkpointPosition, CancellationToken cancellationToken)
    {
        var owned = await jobRepository.SaveProgressAsync(jobId, workerId, processedItems, totalItems, checkpointPosition, cancellationToken);
        if (!owned)
        {
            throw new JobLeaseLostException(jobId);
        }

        metrics.ItemsProcessed(jobType, processedItems - _lastProcessedItems);
        _lastProcessedItems = processedItems;
    }
}
