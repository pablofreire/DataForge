using DataForge.BuildingBlocks.Application.Tenancy;
using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DataForge.BuildingBlocks.Application.DependencyInjection;

public static class BuildingBlocksApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<TenantContext>();
        services.TryAddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
        return services;
    }
}
