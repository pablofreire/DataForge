using DataForge.Modules.Identity.Application.Interfaces;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;
using DataForge.Modules.Identity.Infrastructure.Options;
using DataForge.Modules.Identity.Infrastructure.Persistence.Repositories;
using DataForge.Modules.Identity.Infrastructure.Security;
using DataForge.Modules.Identity.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<ITenantRepository, TenantRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.AddOptions<DemoSeedOptions>().BindConfiguration(DemoSeedOptions.SectionName);
        services.AddScoped<IdentityDemoDataSeeder>();

        return services;
    }
}
