using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Validation.Application.Interfaces;
using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Infrastructure.Persistence.Queries;
using NpgsqlTypes;

namespace DataForge.Modules.Validation.Infrastructure.Persistence.Writers;

/// <summary>Writes issues with binary COPY; one call is one transaction.</summary>
public sealed class PostgresValidationIssueWriter(IDbConnectionFactory connectionFactory) : IValidationIssueWriter
{
    public async Task WriteAsync(long runId, IReadOnlyCollection<ValidationIssue> issues, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(issues);
        if (issues.Count == 0)
        {
            return;
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var importer = await connection.BeginBinaryImportAsync(ValidationIssueQueries.CopyIn, cancellationToken);

        foreach (var issue in issues)
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(runId, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(issue.RowNumber, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(issue.RuleId, NpgsqlDbType.Bigint, cancellationToken);
            await importer.WriteAsync(issue.ColumnName, NpgsqlDbType.Varchar, cancellationToken);
            await importer.WriteAsync(issue.RuleType.ToString(), NpgsqlDbType.Varchar, cancellationToken);
            await importer.WriteAsync(issue.Message, NpgsqlDbType.Varchar, cancellationToken);

            if (issue.Value is null)
            {
                await importer.WriteNullAsync(cancellationToken);
            }
            else
            {
                await importer.WriteAsync(issue.Value, NpgsqlDbType.Varchar, cancellationToken);
            }
        }

        await importer.CompleteAsync(cancellationToken);
    }

    public Task DeleteAllAsync(long runId, CancellationToken cancellationToken) =>
        ExecuteAsync(ValidationIssueQueries.DeleteAll, new { RunId = runId }, cancellationToken);

    public Task DeleteRowIssuesAfterAsync(long runId, long rowNumber, CancellationToken cancellationToken) =>
        ExecuteAsync(ValidationIssueQueries.DeleteRowIssuesAfter, new { RunId = runId, RowNumber = rowNumber }, cancellationToken);

    public Task DeleteRuleIssuesAsync(long runId, long ruleId, CancellationToken cancellationToken) =>
        ExecuteAsync(ValidationIssueQueries.DeleteRuleIssues, new { RunId = runId, RuleId = ruleId }, cancellationToken);

    private async Task ExecuteAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }
}
