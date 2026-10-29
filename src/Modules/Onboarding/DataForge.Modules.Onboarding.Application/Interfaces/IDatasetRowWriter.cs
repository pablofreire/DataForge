using DataForge.Modules.Onboarding.Application.Models;

namespace DataForge.Modules.Onboarding.Application.Interfaces;

/// <summary>
/// Bulk writer for staged rows (PostgreSQL binary COPY in production).
/// </summary>
public interface IDatasetRowWriter
{
    Task WriteBatchAsync(long datasetId, DatasetRowBatch batch, CancellationToken cancellationToken);

    /// <summary>Removes rows after <paramref name="rowNumber"/>, undoing a partially written batch before resuming.</summary>
    Task DeleteRowsAfterAsync(long datasetId, long rowNumber, CancellationToken cancellationToken);
}
