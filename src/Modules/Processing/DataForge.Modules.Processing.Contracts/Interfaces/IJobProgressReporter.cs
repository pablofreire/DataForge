namespace DataForge.Modules.Processing.Contracts.Interfaces;

/// <summary>
/// Persists job progress. A checkpoint must only be reported after the work up to that position
/// has been durably committed, so a resumed attempt can safely continue from it.
/// </summary>
public interface IJobProgressReporter
{
    Task ReportAsync(long processedItems, long? totalItems, long checkpointPosition, CancellationToken cancellationToken);
}
