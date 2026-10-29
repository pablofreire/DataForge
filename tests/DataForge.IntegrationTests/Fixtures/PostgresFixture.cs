using Dapper;
using DataForge.BuildingBlocks.Application.Tenancy;
using DataForge.BuildingBlocks.Infrastructure.DependencyInjection;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Migrations;
using DataForge.Migrator.Composition;
using DataForge.Modules.Identity.Application.DependencyInjection;
using DataForge.Modules.Identity.Infrastructure.DependencyInjection;
using DataForge.Modules.Identity.Infrastructure.Seeding;
using DataForge.Modules.Onboarding.Application.DependencyInjection;
using DataForge.Modules.Onboarding.Infrastructure.DependencyInjection;
using DataForge.Modules.Processing.Application.DependencyInjection;
using DataForge.Modules.Processing.Infrastructure.DependencyInjection;
using DataForge.Modules.Validation.Application.DependencyInjection;
using DataForge.Modules.Validation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(DataForge.IntegrationTests.Fixtures.PostgresFixture))]

namespace DataForge.IntegrationTests.Fixtures;

/// <summary>
/// One disposable PostgreSQL 17 container for the whole test assembly, migrated with the production
/// migrations and seeded with the demo tenants. No in-memory database: tests exercise real SQL, COPY,
/// SKIP LOCKED and partial unique indexes.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    public const string DemoPassword = "Integration!Test2026";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("dataforge")
        .WithUsername("dataforge")
        .WithPassword("dataforge_test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public string StorageRoot { get; } = Path.Combine(Path.GetTempPath(), "dataforge-integration", Guid.NewGuid().ToString("N"));

    public IReadOnlyDictionary<string, string?> Settings => new Dictionary<string, string?>
    {
        ["Database:ConnectionString"] = ConnectionString,
        ["FileStorage:RootPath"] = StorageRoot,
        ["DemoSeed:Password"] = DemoPassword,
        ["Onboarding:ImportBatchSize"] = "1000",
        ["Validation:BatchSize"] = "1000",
        ["Processing:RetryBaseDelay"] = "00:00:00.050",
        ["RateLimiting:LoginPermitsPerMinute"] = "10000",
    };

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var services = BuildServices();
        var result = services.GetRequiredService<DatabaseMigrator>().Migrate(ConnectionString, MigrationAssemblies.All);
        if (!result.Successful)
        {
            throw new InvalidOperationException($"Migrations failed: {result.ErrorMessage}");
        }

        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IdentityDemoDataSeeder>().SeedAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        if (Directory.Exists(StorageRoot))
        {
            Directory.Delete(StorageRoot, recursive: true);
        }
    }

    /// <summary>The same composition as the Worker host, without the hosted dispatcher (tests drive jobs explicitly).</summary>
    public ServiceProvider BuildServices()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(Settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddMetrics();
        services
            .AddBuildingBlocksInfrastructure(configuration)
            .AddIdentityApplication()
            .AddIdentityInfrastructure()
            .AddOnboardingApplication()
            .AddOnboardingInfrastructure()
            .AddValidationApplication()
            .AddValidationInfrastructure()
            .AddProcessingApplication()
            .AddProcessingInfrastructure();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    /// <summary>Creates a DI scope bound to a seeded tenant ("acme", "globex" or "dataforge-internal").</summary>
    public async Task<AsyncServiceScope> CreateTenantScopeAsync(IServiceProvider services, string tenantSlug)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        var tenant = await connection.QuerySingleAsync<(long Id, Guid PublicId)>(
            "SELECT id, public_id FROM identity.tenants WHERE slug = @Slug", new { Slug = tenantSlug });

        var scope = services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TenantContext>().SetTenant(tenant.Id, tenant.PublicId);
        return scope;
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        return await connection.QuerySingleAsync<T>(sql, parameters);
    }
}
