using DataForge.BuildingBlocks.Infrastructure.DependencyInjection;
using DataForge.BuildingBlocks.Infrastructure.Options;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Migrations;
using DataForge.Migrator.Composition;
using DataForge.Modules.Identity.Infrastructure.DependencyInjection;
using DataForge.Modules.Identity.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

// DataForge.Migrator: applies database migrations and, with --seed-demo, creates the demo tenants and users.
// Runs to completion and exits with a non-zero code on failure, so Docker Compose and CI can gate on it.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration).WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture));
    builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);
    builder.Services.AddIdentityInfrastructure();

    using var host = builder.Build();

    var connectionString = host.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString;
    var migrator = host.Services.GetRequiredService<DatabaseMigrator>();
    var result = migrator.Migrate(connectionString, MigrationAssemblies.All);
    if (!result.Successful)
    {
        return 1;
    }

    if (args.Contains("--seed-demo", StringComparer.OrdinalIgnoreCase))
    {
        await using var scope = host.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IdentityDemoDataSeeder>().SeedAsync(CancellationToken.None);
        Log.Information("Demo data seeded");
    }

    return 0;
}
#pragma warning disable CA1031 // Top-level guard: log and exit with a failure code.
catch (Exception exception)
#pragma warning restore CA1031
{
    Log.Fatal(exception, "Migrator failed");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
