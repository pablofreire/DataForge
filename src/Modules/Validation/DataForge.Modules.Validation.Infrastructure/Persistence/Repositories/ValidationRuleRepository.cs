using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using DataForge.Modules.Validation.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Validation.Infrastructure.Persistence.Repositories;

public sealed class ValidationRuleRepository(IDbConnectionFactory connectionFactory) : IValidationRuleRepository
{
    public async Task AddAsync(ValidationRuleEntity rule, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rule);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            ValidationRuleQueries.Insert,
            new
            {
                rule.PublicId,
                rule.TenantId,
                rule.ProjectPublicId,
                rule.ColumnName,
                RuleType = rule.RuleType.ToString(),
                rule.Parameters,
                rule.CreatedAt,
                rule.UpdatedAt,
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<ValidationRuleEntity>> ListByProjectAsync(long tenantId, Guid projectPublicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rules = await connection.QueryAsync<ValidationRuleEntity>(new CommandDefinition(
            ValidationRuleQueries.ListByProject,
            new { TenantId = tenantId, ProjectPublicId = projectPublicId },
            cancellationToken: cancellationToken));
        return rules.AsList();
    }

    public async Task<bool> DeactivateAsync(long tenantId, Guid rulePublicId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            ValidationRuleQueries.Deactivate,
            new { TenantId = tenantId, PublicId = rulePublicId, Now = now },
            cancellationToken: cancellationToken));
        return affected == 1;
    }
}
