using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;

namespace DataForge.BuildingBlocks.Application.Tenancy;

/// <summary>
/// Scoped, set-once implementation of <see cref="ITenantContext"/>.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private long? _tenantId;
    private Guid _tenantPublicId;

    public bool HasTenant => _tenantId.HasValue;

    public long TenantId => _tenantId ?? throw new InvalidOperationException("No tenant has been resolved for the current scope.");

    public Guid TenantPublicId => HasTenant
        ? _tenantPublicId
        : throw new InvalidOperationException("No tenant has been resolved for the current scope.");

    public void SetTenant(long tenantId, Guid tenantPublicId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);

        if (_tenantId.HasValue && _tenantId.Value != tenantId)
        {
            throw new InvalidOperationException("The tenant of a scope cannot be changed once it has been set.");
        }

        _tenantId = tenantId;
        _tenantPublicId = tenantPublicId;
    }
}
