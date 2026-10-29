using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Onboarding.Application.Interfaces;
using DataForge.Modules.Onboarding.Application.Models;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;
using NpgsqlTypes;

namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Writers;

/// <summary>
/// Writes staged rows with PostgreSQL binary COPY, the fastest bulk-load path available to a client
/// (no per-row statement parsing, planning or round trip). Each batch is one COPY, i.e. one transaction.
/// </summary>
public sealed class PostgresDatasetRowWriter(IDbConnectionFactory connectionFactory) : IDatasetRowWriter
{
    public async Task WriteBatchAsync(long datasetId, DatasetRowBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var importer = await connection.BeginBinaryImportAsync(DatasetRowQueries.CopyIn, cancellationToken);

        var rowNumber = batch.FirstRowNumber;
        foreach (var values in batch.Rows)
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(datasetId, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(rowNumber, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(values, NpgsqlDbType.Array | NpgsqlDbType.Text, cancellationToken);
            rowNumber++;
        }

        await importer.CompleteAsync(cancellationToken);
    }

    public async Task DeleteRowsAfterAsync(long datasetId, long rowNumber, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            DatasetRowQueries.DeleteAfter,
            new { DatasetId = datasetId, RowNumber = rowNumber },
            cancellationToken: cancellationToken));
    }
}
