using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Identity.Domain.Entities;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;
using DataForge.Modules.Identity.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Identity.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(IDbConnectionFactory connectionFactory) : ITenantRepository
{
    public async Task<TenantEntity?> GetByIdAsync(long tenantId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TenantEntity>(new CommandDefinition(
            TenantQueries.GetById,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<TenantEntity>> GetByIdsAsync(IReadOnlyCollection<long> tenantIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantIds);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var tenants = await connection.QueryAsync<TenantEntity>(new CommandDefinition(
            TenantQueries.GetByIds,
            new { TenantIds = tenantIds.ToArray() },
            cancellationToken: cancellationToken));
        return tenants.AsList();
    }

    public async Task<TenantEntity> AddOrGetBySlugAsync(TenantEntity tenant, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<TenantEntity>(new CommandDefinition(
            TenantQueries.AddOrGetBySlug,
            new { tenant.PublicId, tenant.Name, tenant.Slug, tenant.CreatedAt, tenant.UpdatedAt },
            cancellationToken: cancellationToken));
    }
}
