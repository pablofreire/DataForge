using System.Reflection;
using DataForge.Modules.Identity.Infrastructure.DependencyInjection;
using DataForge.Modules.Onboarding.Infrastructure.DependencyInjection;
using DataForge.Modules.Processing.Infrastructure.DependencyInjection;
using DataForge.Modules.Validation.Infrastructure.DependencyInjection;

namespace DataForge.Migrator.Composition;

/// <summary>The module assemblies that carry embedded SQL migrations. Adding a module means adding it here.</summary>
public static class MigrationAssemblies
{
    public static IReadOnlyList<Assembly> All { get; } =
    [
        typeof(IdentityInfrastructureServiceCollectionExtensions).Assembly,
        typeof(OnboardingInfrastructureServiceCollectionExtensions).Assembly,
        typeof(ValidationInfrastructureServiceCollectionExtensions).Assembly,
        typeof(ProcessingInfrastructureServiceCollectionExtensions).Assembly,
    ];
}
