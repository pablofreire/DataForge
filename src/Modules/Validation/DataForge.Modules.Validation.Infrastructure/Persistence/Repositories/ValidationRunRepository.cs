using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using DataForge.Modules.Validation.Domain.ReadModels;
using DataForge.Modules.Validation.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Validation.Infrastructure.Persistence.Repositories;

public sealed class ValidationRunRepository(IDbConnectionFactory connectionFactory) : IValidationRunRepository
{
    public async Task AddAsync(ValidationRunEntity run, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(run);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            ValidationRunQueries.Insert,
            new
            {
                run.PublicId,
                run.TenantId,
                run.DatasetPublicId,
                run.ProjectPublicId,
                Status = run.Status.ToString(),
                run.CreatedAt,
                run.UpdatedAt,
            },
            cancellationToken: cancellationToken));
    }

    public async Task<ValidationRunEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ValidationRunEntity>(new CommandDefinition(
            ValidationRunQueries.GetByPublicId,
            new { TenantId = tenantId, PublicId = publicId },
            cancellationToken: cancellationToken));
    }

    public async Task<ValidationRunEntity?> GetLatestForDatasetAsync(long tenantId, Guid datasetPublicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ValidationRunEntity>(new CommandDefinition(
            ValidationRunQueries.GetLatestForDataset,
            new { TenantId = tenantId, DatasetPublicId = datasetPublicId },
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(ValidationRunEntity run, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(run);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            ValidationRunQueries.Update,
            new
            {
                run.Id,
                run.TenantId,
                Status = run.Status.ToString(),
                run.TotalRows,
                run.ValidatedRows,
                run.InvalidRows,
                run.IssueCount,
                run.RuleCount,
                run.FailureReason,
                run.StartedAt,
                run.CompletedAt,
                run.UpdatedAt,
            },
            cancellationToken: cancellationToken));
    }

    public async Task<RunTotals> ComputeTotalsAsync(long runId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(ValidationRunQueries.RefreshRuleStats, new { RunId = runId }, transaction, cancellationToken: cancellationToken));
        var totals = await connection.QuerySingleAsync<RunTotals>(new CommandDefinition(ValidationRunQueries.Totals, new { RunId = runId }, transaction, cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        return totals;
    }

    public async Task<IReadOnlyList<RuleIssueStat>> ListRuleStatsAsync(long runId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var stats = await connection.QueryAsync<RuleIssueStat>(new CommandDefinition(
            ValidationRunQueries.ListRuleStats,
            new { RunId = runId },
            cancellationToken: cancellationToken));
        return stats.AsList();
    }
}
