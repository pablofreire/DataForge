using DataForge.Modules.Onboarding.Application.Interfaces;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Onboarding.Infrastructure.Csv;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Readers;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Repositories;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Writers;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Onboarding.Infrastructure.DependencyInjection;

public static class OnboardingInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOnboardingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<IDatasetRepository, DatasetRepository>();
        services.AddSingleton<IDatasetRowWriter, PostgresDatasetRowWriter>();
        services.AddSingleton<ICsvDatasetReader, SepCsvDatasetReader>();
        services.AddScoped<IDatasetRowReader, PostgresDatasetRowReader>();
        return services;
    }
}
