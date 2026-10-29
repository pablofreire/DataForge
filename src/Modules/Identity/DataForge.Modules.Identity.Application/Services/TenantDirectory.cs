using DataForge.Modules.Identity.Contracts.Dtos;
using DataForge.Modules.Identity.Contracts.Interfaces;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Identity.Application.Services;

public sealed class TenantDirectory(ITenantRepository tenantRepository) : ITenantDirectory
{
    public async Task<IReadOnlyDictionary<long, TenantSummaryDto>> GetByIdsAsync(IReadOnlyCollection<long> tenantIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantIds);

        if (tenantIds.Count == 0)
        {
            return new Dictionary<long, TenantSummaryDto>();
        }

        var tenants = await tenantRepository.GetByIdsAsync(tenantIds, cancellationToken);
        return tenants.ToDictionary(tenant => tenant.Id, tenant => new TenantSummaryDto(tenant.PublicId, tenant.Name, tenant.Slug));
    }
}
