using DataForge.BuildingBlocks.Application.Events.Interfaces;
using DataForge.Modules.Onboarding.Contracts.IntegrationEvents;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Validation.Application.IntegrationEventHandlers;
using DataForge.Modules.Validation.Application.Jobs;
using DataForge.Modules.Validation.Application.Options;
using DataForge.Modules.Validation.Application.Services;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Implementations;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Implementations;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Validators;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Implementations;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Implementations;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Implementations;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Implementations;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Validation.Application.DependencyInjection;

public static class ValidationApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddValidationApplication(this IServiceCollection services)
    {
        services.AddOptions<ValidationOptions>()
            .BindConfiguration(ValidationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddValidatorsFromAssemblyContaining<CreateValidationRuleRequestValidator>(ServiceLifetime.Singleton);

        services.AddScoped<ICreateValidationRuleUseCase, CreateValidationRuleUseCase>();
        services.AddScoped<IListValidationRulesUseCase, ListValidationRulesUseCase>();
        services.AddScoped<IDeleteValidationRuleUseCase, DeleteValidationRuleUseCase>();
        services.AddScoped<IStartValidationRunUseCase, StartValidationRunUseCase>();
        services.AddScoped<IGetLatestValidationRunUseCase, GetLatestValidationRunUseCase>();
        services.AddScoped<IListValidationIssuesUseCase, ListValidationIssuesUseCase>();

        services.AddScoped<ValidationRunScheduler>();
        services.AddScoped<IJobExecutor, ValidateDatasetJobExecutor>();
        services.AddScoped<IIntegrationEventHandler<DatasetImportedIntegrationEvent>, DatasetImportedIntegrationEventHandler>();

        return services;
    }
}
