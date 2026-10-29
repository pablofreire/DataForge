using DataForge.Modules.Identity.Domain.Entities;

namespace DataForge.Modules.Identity.Domain.Interfaces.Repositories;

public interface ITenantRepository
{
    Task<TenantEntity?> GetByIdAsync(long tenantId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TenantEntity>> GetByIdsAsync(IReadOnlyCollection<long> tenantIds, CancellationToken cancellationToken);

    /// <summary>Inserts the tenant unless the slug exists, and returns the stored tenant either way.</summary>
    Task<TenantEntity> AddOrGetBySlugAsync(TenantEntity tenant, CancellationToken cancellationToken);
}
