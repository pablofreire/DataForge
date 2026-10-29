using System.Globalization;
using System.Security.Claims;
using DataForge.BuildingBlocks.Application.Tenancy;
using DataForge.BuildingBlocks.Web.Security;

namespace DataForge.BuildingBlocks.Web.Tenancy;

/// <summary>
/// Reads tenant claims from a principal. Shared by the HTTP middleware and the Blazor circuit handler,
/// because interactive Blazor components run outside the HTTP request pipeline.
/// </summary>
public static class TenantClaimsReader
{
    public static bool TryApply(ClaimsPrincipal? principal, TenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        if (principal?.Identity?.IsAuthenticated != true || tenantContext.HasTenant)
        {
            return tenantContext.HasTenant;
        }

        var tenantIdValue = principal.FindFirstValue(DataForgeClaimTypes.TenantId);
        var tenantPublicIdValue = principal.FindFirstValue(DataForgeClaimTypes.TenantPublicId);

        if (long.TryParse(tenantIdValue, NumberStyles.None, CultureInfo.InvariantCulture, out var tenantId)
            && Guid.TryParse(tenantPublicIdValue, out var tenantPublicId))
        {
            tenantContext.SetTenant(tenantId, tenantPublicId);
            return true;
        }

        return false;
    }
}
