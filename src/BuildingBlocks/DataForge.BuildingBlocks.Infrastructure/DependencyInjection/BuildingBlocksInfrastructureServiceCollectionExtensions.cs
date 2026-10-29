using DataForge.BuildingBlocks.Application.DependencyInjection;
using DataForge.BuildingBlocks.Application.Events.Interfaces;
using DataForge.BuildingBlocks.Application.Storage.Interfaces;
using DataForge.BuildingBlocks.Infrastructure.Events;
using DataForge.BuildingBlocks.Infrastructure.HealthChecks;
using DataForge.BuildingBlocks.Infrastructure.Options;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Configuration;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Connections;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Migrations;
using DataForge.BuildingBlocks.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DataForge.BuildingBlocks.Infrastructure.DependencyInjection;

public static class BuildingBlocksInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        DapperConfiguration.Configure();

        services.AddBuildingBlocksApplication();

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Database:ConnectionString is required.")
            .ValidateOnStart();

        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName));

        services.TryAddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return new NpgsqlDataSourceBuilder(options.ConnectionString).Build();
        });

        services.TryAddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.TryAddSingleton<IFileStorage, LocalFileStorage>();
        services.TryAddScoped<IIntegrationEventPublisher, InProcessIntegrationEventPublisher>();
        services.TryAddSingleton<DatabaseMigrator>();

        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

        return services;
    }
}
