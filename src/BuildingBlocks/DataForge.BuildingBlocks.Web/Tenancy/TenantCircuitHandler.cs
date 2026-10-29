using DataForge.BuildingBlocks.Application.Tenancy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace DataForge.BuildingBlocks.Web.Tenancy;

/// <summary>
/// Interactive Blazor components run inside a SignalR circuit with its own DI scope, where the HTTP
/// middleware never runs. This handler resolves the tenant for that scope when the circuit opens.
/// </summary>
public sealed class TenantCircuitHandler(
    AuthenticationStateProvider authenticationStateProvider,
    TenantContext tenantContext) : CircuitHandler
{
    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        TenantClaimsReader.TryApply(state.User, tenantContext);
    }
}
