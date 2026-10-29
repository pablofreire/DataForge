using DataForge.Modules.Processing.Application.Diagnostics;
using DataForge.Modules.Processing.Application.Execution;
using DataForge.Modules.Processing.Application.Options;
using DataForge.Modules.Processing.Application.Services;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Implementations;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Implementations;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Validators;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Implementations;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Implementations;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Interfaces;
using DataForge.Modules.Processing.Contracts.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Processing.Application.DependencyInjection;

public static class ProcessingApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddProcessingApplication(this IServiceCollection services)
    {
        services.AddOptions<ProcessingOptions>()
            .BindConfiguration(ProcessingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddValidatorsFromAssemblyContaining<GetJobByIdRequestValidator>(ServiceLifetime.Singleton);
        services.AddSingleton<ProcessingMetrics>();

        services.AddScoped<IJobScheduler, JobScheduler>();
        services.AddScoped<IJobQueryService, JobQueryService>();
        services.AddScoped<JobRunner>();

        services.AddScoped<IGetJobByIdUseCase, GetJobByIdUseCase>();
        services.AddScoped<ICancelJobUseCase, CancelJobUseCase>();
        services.AddScoped<IListRecentJobsUseCase, ListRecentJobsUseCase>();
        services.AddScoped<IListOperationsJobsUseCase, ListOperationsJobsUseCase>();

        return services;
    }
}
