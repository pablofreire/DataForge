using DataForge.BuildingBlocks.Application.Tenancy;
using Microsoft.AspNetCore.Http;

namespace DataForge.BuildingBlocks.Web.Tenancy;

/// <summary>
/// Binds the scoped <see cref="TenantContext"/> to the tenant claims of the authenticated principal.
/// Runs after authentication; anonymous requests leave the context unresolved.
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenantContext);

        TenantClaimsReader.TryApply(context.User, tenantContext);
        return next(context);
    }
}
