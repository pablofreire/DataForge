using DataForge.Modules.Validation.Domain.Entities;

namespace DataForge.Modules.Validation.Domain.Interfaces.Repositories;

public interface IValidationRuleRepository
{
    Task AddAsync(ValidationRuleEntity rule, CancellationToken cancellationToken);

    Task<IReadOnlyList<ValidationRuleEntity>> ListByProjectAsync(long tenantId, Guid projectPublicId, CancellationToken cancellationToken);

    /// <summary>Soft-deletes the rule. Returns false when it does not exist for the tenant.</summary>
    Task<bool> DeactivateAsync(long tenantId, Guid rulePublicId, DateTimeOffset now, CancellationToken cancellationToken);
}
