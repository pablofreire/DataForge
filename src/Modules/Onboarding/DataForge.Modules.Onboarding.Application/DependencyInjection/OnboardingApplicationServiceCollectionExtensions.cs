using DataForge.Modules.Onboarding.Application.Jobs;
using DataForge.Modules.Onboarding.Application.Options;
using DataForge.Modules.Onboarding.Application.Services;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Validators;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Implementations;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Interfaces;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Onboarding.Application.DependencyInjection;

public static class OnboardingApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddOnboardingApplication(this IServiceCollection services)
    {
        services.AddOptions<OnboardingOptions>()
            .BindConfiguration(OnboardingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddValidatorsFromAssemblyContaining<CreateProjectRequestValidator>(ServiceLifetime.Singleton);

        services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
        services.AddScoped<IListProjectsUseCase, ListProjectsUseCase>();
        services.AddScoped<IGetProjectByIdUseCase, GetProjectByIdUseCase>();
        services.AddScoped<IUploadDatasetUseCase, UploadDatasetUseCase>();
        services.AddScoped<IGetDatasetByIdUseCase, GetDatasetByIdUseCase>();
        services.AddScoped<IListProjectDatasetsUseCase, ListProjectDatasetsUseCase>();
        services.AddScoped<IGetDatasetRowsUseCase, GetDatasetRowsUseCase>();

        services.AddScoped<IOnboardingModuleApi, OnboardingModuleApi>();
        services.AddScoped<IJobExecutor, ImportDatasetJobExecutor>();

        return services;
    }
}
