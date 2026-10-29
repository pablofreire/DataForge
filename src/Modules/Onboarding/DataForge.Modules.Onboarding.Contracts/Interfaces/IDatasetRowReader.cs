using DataForge.Modules.Onboarding.Contracts.Dtos;

namespace DataForge.Modules.Onboarding.Contracts.Interfaces;

/// <summary>
/// Streaming access to staged dataset rows for other modules. The Onboarding module owns the storage
/// layout and the SQL; consumers only see rows and row numbers.
/// </summary>
public interface IDatasetRowReader
{
    /// <summary>Keyset-paginated read: rows with <c>RowNumber &gt; afterRowNumber</c>, in order.</summary>
    Task<IReadOnlyList<DatasetRowDto>> ReadBatchAsync(Guid datasetId, long afterRowNumber, int batchSize, CancellationToken cancellationToken);

    /// <summary>Streams rows whose value in <paramref name="columnIndex"/> (0-based) appears more than once, ignoring blanks.</summary>
    IAsyncEnumerable<DuplicateValueDto> ReadDuplicatesAsync(Guid datasetId, int columnIndex, CancellationToken cancellationToken);
}
