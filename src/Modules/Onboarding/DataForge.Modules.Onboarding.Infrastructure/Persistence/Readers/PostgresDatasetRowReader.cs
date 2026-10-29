using System.Runtime.CompilerServices;
using Dapper;
using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Onboarding.Contracts.Dtos;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Readers;

public sealed class PostgresDatasetRowReader(IDbConnectionFactory connectionFactory, ITenantContext tenantContext) : IDatasetRowReader
{
    public async Task<IReadOnlyList<DatasetRowDto>> ReadBatchAsync(Guid datasetId, long afterRowNumber, int batchSize, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var internalId = await ResolveAsync(connection, datasetId, cancellationToken);
        if (internalId is null)
        {
            return [];
        }

        var rows = await connection.QueryAsync<(long RowNumber, string?[] Values)>(new CommandDefinition(
            DatasetRowQueries.ReadBatch,
            new { DatasetId = internalId.Value, AfterRowNumber = afterRowNumber, BatchSize = batchSize },
            cancellationToken: cancellationToken));

        return rows.Select(row => new DatasetRowDto(row.RowNumber, row.Values)).ToList();
    }

    public async IAsyncEnumerable<DuplicateValueDto> ReadDuplicatesAsync(
        Guid datasetId,
        int columnIndex,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(columnIndex);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var internalId = await ResolveAsync(connection, datasetId, cancellationToken);
        if (internalId is null)
        {
            yield break;
        }

        var duplicates = connection.QueryUnbufferedAsync<(long RowNumber, string Value)>(
            DatasetRowQueries.ReadDuplicates,
            new { DatasetId = internalId.Value, Ordinal = columnIndex + 1 });

        await foreach (var duplicate in duplicates.WithCancellation(cancellationToken))
        {
            yield return new DuplicateValueDto(duplicate.RowNumber, duplicate.Value);
        }
    }

    private Task<long?> ResolveAsync(Npgsql.NpgsqlConnection connection, Guid datasetId, CancellationToken cancellationToken) =>
        connection.ExecuteScalarAsync<long?>(new CommandDefinition(
            DatasetRowQueries.ResolveDatasetId,
            new { tenantContext.TenantId, PublicId = datasetId },
            cancellationToken: cancellationToken));
}
