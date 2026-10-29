using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using DataForge.Modules.Validation.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Validation.Infrastructure.Persistence.Repositories;

public sealed class ValidationIssueRepository(IDbConnectionFactory connectionFactory) : IValidationIssueRepository
{
    public async Task<IReadOnlyList<ValidationIssue>> ListAsync(
        long runId,
        long afterRowNumber,
        long afterRuleId,
        int limit,
        string? columnName,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<(long RowNumber, long RuleId, string ColumnName, string RuleType, string Message, string? Value)>(
            new CommandDefinition(
                ValidationIssueQueries.List,
                new { RunId = runId, AfterRowNumber = afterRowNumber, AfterRuleId = afterRuleId, Limit = limit, ColumnName = columnName },
                cancellationToken: cancellationToken));

        return rows
            .Select(row => new ValidationIssue(row.RowNumber, row.RuleId, row.ColumnName, Enum.Parse<RuleType>(row.RuleType), row.Message, row.Value))
            .ToList();
    }
}
