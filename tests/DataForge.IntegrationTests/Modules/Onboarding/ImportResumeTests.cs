using DataForge.IntegrationTests.Fixtures;
using DataForge.Modules.Onboarding.Application.Interfaces;
using DataForge.Modules.Onboarding.Application.Jobs;
using DataForge.Modules.Onboarding.Application.Models;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Interfaces;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Models;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DataForge.IntegrationTests.Modules.Onboarding;

/// <summary>
/// Simulates a worker that crashed in the middle of a batch: rows up to the checkpoint are committed, and rows
/// after it were partially written with stale data. Resuming must produce exactly the rows of the file.
/// </summary>
public sealed class ImportResumeTests(PostgresFixture database)
{
    [Fact]
    public async Task Resume_FromCheckpoint_DiscardsPartialBatchAndProducesIdenticalRows()
    {
        const int rows = 5_000;
        const long checkpoint = 3_000;
        await using var services = database.BuildServices();
        await using var scope = await database.CreateTenantScopeAsync(services, "acme");
        var provider = scope.ServiceProvider;

        var project = await provider.GetRequiredService<ICreateProjectUseCase>()
            .ExecuteAsync(new CreateProjectRequest($"Resume {Guid.NewGuid():N}", null), CancellationToken.None);
        var upload = await provider.GetRequiredService<IUploadDatasetUseCase>()
            .ExecuteAsync(new UploadDatasetRequest(project.Value.ProjectId, "big.csv", CsvSamples.ToStream(CsvSamples.Generated(rows))), CancellationToken.None);
        var datasetId = upload.Value.Dataset.DatasetId;
        var internalId = await database.QuerySingleAsync<long>("SELECT id FROM onboarding.datasets WHERE public_id = @Id", new { Id = datasetId });

        // State left by the crashed attempt: committed rows 1..3000 plus a partially written batch 3001..3500 with stale values.
        var writer = provider.GetRequiredService<IDatasetRowWriter>();
        await writer.WriteBatchAsync(internalId, new DatasetRowBatch(1, Rows(1, 3_000)), CancellationToken.None);
        await writer.WriteBatchAsync(internalId, new DatasetRowBatch(3_001, Enumerable.Range(0, 500).Select(_ => new[] { "stale", "stale", "stale" }).ToList()), CancellationToken.None);

        // Take the queued import job off the queue so only this resumed attempt runs it.
        await provider.GetRequiredService<IJobRepository>().TryClaimNextAsync("crashed-worker", TimeSpan.FromMinutes(5), CancellationToken.None);

        var executor = provider.GetServices<IJobExecutor>().Single(candidate => candidate.JobType == OnboardingJobTypes.ImportDataset);
        await executor.ExecuteAsync(new JobExecutionContext(Guid.NewGuid(), datasetId, Attempt: 2, checkpoint, new NoOpProgress()), CancellationToken.None);

        var result = await database.QuerySingleAsync<(long Count, long Stale, long Max)>("""
            SELECT count(*), count(*) FILTER (WHERE values[1] = 'stale'), max(row_number)
            FROM onboarding.dataset_rows WHERE dataset_id = @Id
            """, new { Id = internalId });
        result.Should().Be((rows, 0L, rows));

        var row3001 = await database.QuerySingleAsync<string>(
            "SELECT values[2] FROM onboarding.dataset_rows WHERE dataset_id = @Id AND row_number = 3001", new { Id = internalId });
        row3001.Should().Be("user3001@example.com");

        await JobDriver.RunUntilIdleAsync(services);
    }

    private static List<string[]> Rows(int from, int to) =>
        Enumerable.Range(from, to - from + 1)
            .Select(row => new[] { row.ToString(System.Globalization.CultureInfo.InvariantCulture), $"user{row}@example.com", "1.50" })
            .ToList();

    private sealed class NoOpProgress : IJobProgressReporter
    {
        public Task ReportAsync(long processedItems, long? totalItems, long checkpointPosition, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
