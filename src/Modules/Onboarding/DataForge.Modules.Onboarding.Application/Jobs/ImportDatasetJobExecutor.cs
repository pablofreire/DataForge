using System.Runtime.CompilerServices;
using DataForge.BuildingBlocks.Application.Events.Interfaces;
using DataForge.BuildingBlocks.Application.Pipelines;
using DataForge.BuildingBlocks.Application.Storage.Interfaces;
using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Onboarding.Application.Interfaces;
using DataForge.Modules.Onboarding.Application.Models;
using DataForge.Modules.Onboarding.Application.Options;
using DataForge.Modules.Onboarding.Contracts.IntegrationEvents;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Enums;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Onboarding.Domain.Services;
using DataForge.Modules.Processing.Contracts.Exceptions;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Models;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Onboarding.Application.Jobs;

/// <summary>
/// Streams an uploaded CSV into <c>onboarding.dataset_rows</c>.
/// <list type="number">
/// <item>The CSV reader produces fixed-size batches into a bounded channel (memory stays constant).</item>
/// <item>A single writer drains the channel with binary COPY and commits a checkpoint after each batch.</item>
/// <item>On retry, rows after the last checkpoint are deleted and the reader skips rows up to it, so the
/// final table content is identical whether the job ran once or was resumed (idempotency).</item>
/// </list>
/// </summary>
public sealed class ImportDatasetJobExecutor(
    IDatasetRepository datasetRepository,
    IDatasetRowWriter rowWriter,
    ICsvDatasetReader csvReader,
    IFileStorage fileStorage,
    IIntegrationEventPublisher eventPublisher,
    ITenantContext tenantContext,
    IOptions<OnboardingOptions> options,
    TimeProvider timeProvider) : IJobExecutor
{
    public string JobType => OnboardingJobTypes.ImportDataset;

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var dataset = await datasetRepository.GetByPublicIdAsync(tenantContext.TenantId, context.ResourceId, cancellationToken)
            ?? throw new NonRetryableJobException("The dataset no longer exists.");

        if (dataset.Status == DatasetStatus.Imported)
        {
            return;
        }

        if (dataset.Status == DatasetStatus.Failed)
        {
            throw new NonRetryableJobException(dataset.FailureReason ?? "The dataset is in a failed state.");
        }

        dataset.StartImport(timeProvider.GetUtcNow());
        await datasetRepository.UpdateImportStateAsync(dataset, cancellationToken);

        var checkpoint = context.CheckpointPosition;
        await rowWriter.DeleteRowsAfterAsync(dataset.Id, checkpoint, cancellationToken);

        long lastRowNumber;
        try
        {
            lastRowNumber = await ImportRowsAsync(dataset, context, checkpoint, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            await FailAsync(dataset, exception.Message);
            throw new NonRetryableJobException(exception.Message, exception);
        }

        dataset.CompleteImport(lastRowNumber, timeProvider.GetUtcNow());
        await datasetRepository.UpdateImportStateAsync(dataset, cancellationToken);

        await eventPublisher.PublishAsync(
            new DatasetImportedIntegrationEvent(Guid.CreateVersion7(), timeProvider.GetUtcNow(), dataset.PublicId, dataset.ProjectPublicId, lastRowNumber),
            cancellationToken);
    }

    private async Task<long> ImportRowsAsync(DatasetEntity dataset, JobExecutionContext context, long checkpoint, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        await using var content = await fileStorage.OpenReadAsync(dataset.StoragePath, cancellationToken);
        using var document = csvReader.Open(content);

        var headerError = DatasetHeaderValidator.Validate(document.Columns);
        if (headerError is not null)
        {
            throw new InvalidDataException(headerError);
        }

        if (checkpoint == 0 || dataset.Columns.Length == 0)
        {
            dataset.DefineColumns(document.Columns);
            await datasetRepository.UpdateImportStateAsync(dataset, cancellationToken);
        }

        var lastRowNumber = checkpoint;
        await BoundedChannelPipeline.RunAsync(
            ReadBatchesAsync(document, checkpoint, settings.ImportBatchSize, cancellationToken),
            async (batch, token) =>
            {
                await rowWriter.WriteBatchAsync(dataset.Id, batch, token);
                lastRowNumber = batch.LastRowNumber;
                await context.Progress.ReportAsync(lastRowNumber, null, lastRowNumber, token);
            },
            settings.ImportChannelCapacity,
            cancellationToken);

        return lastRowNumber;
    }

    private static async IAsyncEnumerable<DatasetRowBatch> ReadBatchesAsync(
        ICsvDocument document,
        long skipRows,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Parsing is synchronous and CPU-bound; yield first so it runs on the pipeline's producer task.
        await Task.Yield();

        var rowNumber = 0L;
        var firstRowNumber = skipRows + 1;
        var rows = new List<string[]>(batchSize);

        foreach (var row in document.ReadRows())
        {
            rowNumber++;
            if (rowNumber <= skipRows)
            {
                continue;
            }

            rows.Add(row);
            if (rows.Count == batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new DatasetRowBatch(firstRowNumber, rows);
                firstRowNumber += rows.Count;
                rows = new List<string[]>(batchSize);
            }
        }

        if (rows.Count > 0)
        {
            yield return new DatasetRowBatch(firstRowNumber, rows);
        }
    }

    private async Task FailAsync(DatasetEntity dataset, string reason)
    {
        dataset.Fail(reason, timeProvider.GetUtcNow());
        await datasetRepository.UpdateImportStateAsync(dataset, CancellationToken.None);
    }
}
