using DataForge.IntegrationTests.Fixtures;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.IntegrationTests.Modules.Onboarding;

/// <summary>
/// End-to-end backend flow through real use cases, the real job queue and PostgreSQL:
/// upload, streaming import with binary COPY, integration event, validation with set-based uniqueness.
/// </summary>
public sealed class OnboardingPipelineTests(PostgresFixture database)
{
    [Fact]
    public async Task UploadImportAndValidate_ReportsEveryKnownViolation()
    {
        await using var services = database.BuildServices();
        Guid datasetId;

        await using (var scope = await database.CreateTenantScopeAsync(services, "acme"))
        {
            var projectId = await CreateProjectAsync(scope.ServiceProvider);
            await AddRulesAsync(
                scope.ServiceProvider,
                projectId,
                new RuleSpec("name", "Required"),
                new RuleSpec("email", "Email"),
                new RuleSpec("country", "AllowedValues", AllowedValues: ["BR", "PT", "US"]),
                new RuleSpec("credit_limit", "Numeric", Min: 0, Max: 100_000),
                new RuleSpec("signup_date", "Date"),
                new RuleSpec("customer_id", "Unique"),
                new RuleSpec("segment", "Required"));

            datasetId = await UploadAsync(scope.ServiceProvider, projectId, CsvSamples.CustomersWithErrors);
        }

        var jobs = await JobDriver.RunUntilIdleAsync(services);
        jobs.Should().Be(2, "the import job publishes an event that schedules exactly one validation job");

        await using (var scope = await database.CreateTenantScopeAsync(services, "acme"))
        {
            var dataset = await scope.ServiceProvider.GetRequiredService<IGetDatasetByIdUseCase>()
                .ExecuteAsync(new GetDatasetByIdRequest(datasetId), CancellationToken.None);
            dataset.Value.Dataset.Status.Should().Be("Imported");
            dataset.Value.Dataset.RowCount.Should().Be(5);
            dataset.Value.Dataset.Columns.Should().Equal("customer_id", "name", "email", "country", "credit_limit", "signup_date");

            var run = (await scope.ServiceProvider.GetRequiredService<IGetLatestValidationRunUseCase>()
                .ExecuteAsync(new GetLatestValidationRunRequest(datasetId), CancellationToken.None)).Value;
            run.Run!.Status.Should().Be("Completed");
            run.Run.TotalRows.Should().Be(5);
            run.Run.InvalidRows.Should().Be(5);
            run.Run.IssueCount.Should().Be(10);
            run.RuleStats.Sum(stat => stat.IssueCount).Should().Be(10);

            var issues = (await scope.ServiceProvider.GetRequiredService<IListValidationIssuesUseCase>()
                .ExecuteAsync(new ListValidationIssuesRequest(datasetId, null, 100, null), CancellationToken.None)).Value.Issues;
            issues.Select(issue => (issue.RowNumber, issue.ColumnName, issue.RuleType)).Should().BeEquivalentTo(
            [
                (0L, "segment", "Required"),
                (1L, "customer_id", "Unique"),
                (2L, "email", "Email"),
                (2L, "signup_date", "Date"),
                (3L, "name", "Required"),
                (3L, "credit_limit", "Numeric"),
                (4L, "country", "AllowedValues"),
                (4L, "customer_id", "Unique"),
                (5L, "credit_limit", "Numeric"),
                (5L, "signup_date", "Date"),
            ]);
        }
    }

    [Fact]
    public async Task LargerFile_IsImportedInBatchesWithContiguousRowNumbers()
    {
        const int rows = 25_000;
        await using var services = database.BuildServices();
        Guid datasetId;

        await using (var scope = await database.CreateTenantScopeAsync(services, "acme"))
        {
            var projectId = await CreateProjectAsync(scope.ServiceProvider);
            await AddRulesAsync(scope.ServiceProvider, projectId, new RuleSpec("email", "Email"), new RuleSpec("id", "Unique"));
            datasetId = await UploadAsync(scope.ServiceProvider, projectId, CsvSamples.Generated(rows));
        }

        await JobDriver.RunUntilIdleAsync(services);

        var staged = await database.QuerySingleAsync<(long Count, long Min, long Max, long Distinct)>("""
            SELECT count(*), min(r.row_number), max(r.row_number), count(DISTINCT r.row_number)
            FROM onboarding.dataset_rows r JOIN onboarding.datasets d ON d.id = r.dataset_id
            WHERE d.public_id = @DatasetId
            """, new { DatasetId = datasetId });
        staged.Should().Be((rows, 1L, rows, rows));

        await using (var scope = await database.CreateTenantScopeAsync(services, "acme"))
        {
            var run = (await scope.ServiceProvider.GetRequiredService<IGetLatestValidationRunUseCase>()
                .ExecuteAsync(new GetLatestValidationRunRequest(datasetId), CancellationToken.None)).Value.Run!;
            run.InvalidRows.Should().Be(rows / 10, "every tenth generated row has an invalid e-mail");
            run.IssueCount.Should().Be(rows / 10, "ids are unique, so only the e-mail rule fires");
        }
    }

    [Fact]
    public async Task UploadingTheSameContentTwice_ReturnsTheExistingDataset()
    {
        await using var services = database.BuildServices();
        await using var scope = await database.CreateTenantScopeAsync(services, "acme");
        var projectId = await CreateProjectAsync(scope.ServiceProvider);
        var upload = scope.ServiceProvider.GetRequiredService<IUploadDatasetUseCase>();

        var first = await upload.ExecuteAsync(new UploadDatasetRequest(projectId, "a.csv", CsvSamples.ToStream("id\n1\n")), CancellationToken.None);
        var second = await upload.ExecuteAsync(new UploadDatasetRequest(projectId, "b.csv", CsvSamples.ToStream("id\n1\n")), CancellationToken.None);

        second.Value.IsDuplicate.Should().BeTrue();
        second.Value.Dataset.DatasetId.Should().Be(first.Value.Dataset.DatasetId);
        second.Value.ImportJobId.Should().Be(first.Value.ImportJobId, "enqueueing is idempotent per dataset");

        await JobDriver.RunUntilIdleAsync(services);
    }

    [Fact]
    public async Task Revalidation_CreatesANewRunWithFreshResults()
    {
        await using var services = database.BuildServices();
        Guid datasetId;
        await using (var scope = await database.CreateTenantScopeAsync(services, "acme"))
        {
            var projectId = await CreateProjectAsync(scope.ServiceProvider);
            await AddRulesAsync(scope.ServiceProvider, projectId, new RuleSpec("name", "Required"));
            datasetId = await UploadAsync(scope.ServiceProvider, projectId, CsvSamples.CustomersWithErrors);
        }

        await JobDriver.RunUntilIdleAsync(services);

        await using (var scope = await database.CreateTenantScopeAsync(services, "acme"))
        {
            var started = await scope.ServiceProvider.GetRequiredService<IStartValidationRunUseCase>()
                .ExecuteAsync(new StartValidationRunRequest(datasetId), CancellationToken.None);
            started.IsSuccess.Should().BeTrue();
        }

        (await JobDriver.RunUntilIdleAsync(services)).Should().Be(1);

        var runs = await database.QuerySingleAsync<long>(
            "SELECT count(*) FROM validation.runs WHERE dataset_public_id = @DatasetId AND status = 'Completed'", new { DatasetId = datasetId });
        runs.Should().Be(2);
    }

    private static async Task<Guid> CreateProjectAsync(IServiceProvider services)
    {
        var result = await services.GetRequiredService<ICreateProjectUseCase>()
            .ExecuteAsync(new CreateProjectRequest($"Integration {Guid.NewGuid():N}", null), CancellationToken.None);
        return result.Value.ProjectId;
    }

    private static async Task AddRulesAsync(IServiceProvider services, Guid projectId, params RuleSpec[] rules)
    {
        var createRule = services.GetRequiredService<ICreateValidationRuleUseCase>();
        foreach (var rule in rules)
        {
            var result = await createRule.ExecuteAsync(
                new CreateValidationRuleRequest(projectId, rule.Column, rule.Type, Min: rule.Min, Max: rule.Max, AllowedValues: rule.AllowedValues),
                CancellationToken.None);
            result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Message : null);
        }
    }

    private static async Task<Guid> UploadAsync(IServiceProvider services, Guid projectId, string csv)
    {
        var result = await services.GetRequiredService<IUploadDatasetUseCase>()
            .ExecuteAsync(new UploadDatasetRequest(projectId, "customers.csv", CsvSamples.ToStream(csv)), CancellationToken.None);
        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Message : null);
        return result.Value.Dataset.DatasetId;
    }

    private sealed record RuleSpec(string Column, string Type, decimal? Min = null, decimal? Max = null, IReadOnlyList<string>? AllowedValues = null);
}
