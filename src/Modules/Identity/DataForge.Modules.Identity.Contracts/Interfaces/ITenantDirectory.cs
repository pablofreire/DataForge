using DataForge.Modules.Identity.Contracts.Dtos;

namespace DataForge.Modules.Identity.Contracts.Interfaces;

/// <summary>
/// Cross-tenant lookup of tenant names, used by internal tooling (implementation console).
/// </summary>
public interface ITenantDirectory
{
    Task<IReadOnlyDictionary<long, TenantSummaryDto>> GetByIdsAsync(IReadOnlyCollection<long> tenantIds, CancellationToken cancellationToken);
}
