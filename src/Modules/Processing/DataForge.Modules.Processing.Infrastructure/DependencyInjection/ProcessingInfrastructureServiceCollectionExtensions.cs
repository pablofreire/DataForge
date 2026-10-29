using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using DataForge.Modules.Processing.Infrastructure.Persistence.Repositories;
using DataForge.Modules.Processing.Infrastructure.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.Modules.Processing.Infrastructure.DependencyInjection;

public static class ProcessingInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddProcessingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IJobRepository, JobRepository>();
        return services;
    }

    /// <summary>Registers the queue dispatcher. Only the worker host calls this.</summary>
    public static IServiceCollection AddProcessingWorker(this IServiceCollection services)
    {
        services.AddHostedService<JobDispatcherWorker>();
        return services;
    }
}
