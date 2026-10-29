using DataForge.BuildingBlocks.Infrastructure.Persistence.Migrations;
using DataForge.IntegrationTests.Fixtures;
using DataForge.Migrator.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.IntegrationTests.Persistence;

public sealed class MigrationTests(PostgresFixture database)
{
    [Fact]
    public void Migrate_SecondRun_IsANoOp()
    {
        using var services = database.BuildServices();

        var result = services.GetRequiredService<DatabaseMigrator>().Migrate(database.ConnectionString, MigrationAssemblies.All);

        result.Successful.Should().BeTrue();
        result.AppliedScripts.Should().BeEmpty("every script was journaled by the first run");
    }

    [Fact]
    public async Task Migrate_CreatesOneSchemaPerModule()
    {
        var schemas = await database.QuerySingleAsync<string>(
            "SELECT string_agg(schema_name, ',' ORDER BY schema_name) FROM information_schema.schemata WHERE schema_name IN ('identity','onboarding','validation','processing')");

        schemas.Should().Be("identity,onboarding,processing,validation");
    }

    [Fact]
    public async Task Migrate_EveryTableWithPublicIdHasTheRequiredIndexes()
    {
        var missing = await database.QuerySingleAsync<long>("""
            SELECT count(*)
            FROM information_schema.columns c
            WHERE c.column_name = 'public_id'
              AND c.table_schema IN ('identity','onboarding','validation','processing')
              AND NOT EXISTS (
                  SELECT 1 FROM pg_indexes i
                  WHERE i.schemaname = c.table_schema AND i.tablename = c.table_name
                    AND i.indexname = 'ix_' || c.table_name || '_public_id')
            """);

        missing.Should().Be(0);
    }
}
