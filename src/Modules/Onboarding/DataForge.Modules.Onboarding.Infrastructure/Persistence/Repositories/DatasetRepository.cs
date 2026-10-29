using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Onboarding.Domain.ReadModels;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class DatasetRepository(IDbConnectionFactory connectionFactory) : IDatasetRepository
{
    public async Task<bool> AddAsync(DatasetEntity dataset, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var id = await connection.ExecuteScalarAsync<long?>(new CommandDefinition(
            DatasetQueries.Insert,
            new
            {
                dataset.PublicId,
                dataset.TenantId,
                dataset.ProjectId,
                dataset.FileName,
                dataset.ContentSha256,
                dataset.SizeBytes,
                dataset.StoragePath,
                Status = dataset.Status.ToString(),
                dataset.CreatedAt,
                dataset.UpdatedAt,
            },
            cancellationToken: cancellationToken));
        return id.HasValue;
    }

    public async Task<DatasetEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<DatasetEntity>(new CommandDefinition(
            DatasetQueries.GetByPublicId,
            new { TenantId = tenantId, PublicId = publicId },
            cancellationToken: cancellationToken));
    }

    public async Task<DatasetEntity?> GetByContentHashAsync(long tenantId, long projectId, string contentSha256, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<DatasetEntity>(new CommandDefinition(
            DatasetQueries.GetByContentHash,
            new { TenantId = tenantId, ProjectId = projectId, ContentSha256 = contentSha256 },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<DatasetEntity>> ListByProjectAsync(long tenantId, long projectId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var datasets = await connection.QueryAsync<DatasetEntity>(new CommandDefinition(
            DatasetQueries.ListByProject,
            new { TenantId = tenantId, ProjectId = projectId },
            cancellationToken: cancellationToken));
        return datasets.AsList();
    }

    public async Task UpdateImportStateAsync(DatasetEntity dataset, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            DatasetQueries.UpdateImportState,
            new
            {
                dataset.Id,
                dataset.TenantId,
                Status = dataset.Status.ToString(),
                dataset.RowCount,
                dataset.Columns,
                dataset.FailureReason,
                dataset.ImportedAt,
                dataset.UpdatedAt,
            },
            cancellationToken: cancellationToken));
    }

    public async Task<DatasetTotals> GetTotalsAsync(long tenantId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<DatasetTotals>(new CommandDefinition(
            DatasetQueries.Totals,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));
    }
}
