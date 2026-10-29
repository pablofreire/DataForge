using System.Runtime.CompilerServices;
using DataForge.BuildingBlocks.Application.Pipelines;
using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Onboarding.Contracts.Dtos;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Exceptions;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Models;
using DataForge.Modules.Validation.Application.Interfaces;
using DataForge.Modules.Validation.Application.Options;
using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Domain.Engine.Services;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Validation.Application.Jobs;

/// <summary>
/// Validates a staged dataset against the project's rules.
/// <list type="number">
/// <item>A producer reads rows with keyset pagination into a bounded channel.</item>
/// <item>The consumer validates each batch in parallel (rows are independent), writes the issues with
/// binary COPY and checkpoints the last row, so a resumed attempt continues where the previous one stopped.</item>
/// <item>Set-based rules (uniqueness) run afterwards as a single SQL aggregation over the whole dataset.</item>
/// <item>Totals and the per-rule breakdown are computed once in the database and stored on the run.</item>
/// </list>
/// </summary>
public sealed class ValidateDatasetJobExecutor(
    IValidationRunRepository runRepository,
    IValidationRuleRepository ruleRepository,
    IValidationIssueWriter issueWriter,
    IOnboardingModuleApi onboardingApi,
    IDatasetRowReader rowReader,
    ITenantContext tenantContext,
    IOptions<ValidationOptions> options,
    TimeProvider timeProvider) : IJobExecutor
{
    private const int DuplicateIssueBatchSize = 5_000;

    public string JobType => ValidationJobTypes.ValidateDataset;

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var run = await runRepository.GetByPublicIdAsync(tenantContext.TenantId, context.ResourceId, cancellationToken)
            ?? throw new NonRetryableJobException("The validation run no longer exists.");

        if (run.Status is ValidationRunStatus.Completed or ValidationRunStatus.Failed)
        {
            return;
        }

        var dataset = await onboardingApi.GetDatasetAsync(run.DatasetPublicId, cancellationToken);
        if (dataset is null || dataset.Status != "Imported")
        {
            await FailAsync(run, "The dataset is not available for validation.");
            throw new NonRetryableJobException("The dataset is not available for validation.");
        }

        var settings = options.Value;
        var rules = await ruleRepository.ListByProjectAsync(tenantContext.TenantId, run.ProjectPublicId, cancellationToken);
        var compilation = RuleCompiler.Compile(rules, dataset.Columns, settings.RegexTimeout);
        var rowRules = compilation.RowRules.ToArray();

        var checkpoint = context.CheckpointPosition;
        run.Start(dataset.RowCount, rules.Count, timeProvider.GetUtcNow());
        await runRepository.UpdateAsync(run, cancellationToken);

        if (checkpoint == 0)
        {
            await issueWriter.DeleteAllAsync(run.Id, cancellationToken);
            if (compilation.MissingColumnIssues.Count > 0)
            {
                await issueWriter.WriteAsync(run.Id, compilation.MissingColumnIssues.ToArray(), cancellationToken);
            }
        }
        else
        {
            await issueWriter.DeleteRowIssuesAfterAsync(run.Id, checkpoint, cancellationToken);
        }

        if (rowRules.Length > 0)
        {
            await BoundedChannelPipeline.RunAsync(
                ReadBatchesAsync(dataset.DatasetId, checkpoint, settings.BatchSize, cancellationToken),
                async (batch, token) =>
                {
                    var issues = ValidateBatch(batch, rowRules, settings.EffectiveDegreeOfParallelism, token);
                    if (issues.Count > 0)
                    {
                        await issueWriter.WriteAsync(run.Id, issues, token);
                    }

                    var lastRow = batch[^1].RowNumber;
                    await context.Progress.ReportAsync(lastRow, dataset.RowCount, lastRow, token);
                    run.RecordProgress(lastRow, timeProvider.GetUtcNow());
                    await runRepository.UpdateAsync(run, token);
                },
                settings.ChannelCapacity,
                cancellationToken);
        }

        foreach (var setRule in compilation.SetRules)
        {
            await RunUniqueRuleAsync(run.Id, dataset.DatasetId, setRule, cancellationToken);
        }

        var totals = await runRepository.ComputeTotalsAsync(run.Id, cancellationToken);
        run.Complete(totals.InvalidRows, totals.IssueCount, timeProvider.GetUtcNow());
        await runRepository.UpdateAsync(run, cancellationToken);
        await context.Progress.ReportAsync(dataset.RowCount, dataset.RowCount, dataset.RowCount, cancellationToken);
    }

    private static List<ValidationIssue> ValidateBatch(
        IReadOnlyList<DatasetRowDto> batch,
        IReadOnlyList<CompiledRule> rowRules,
        int degreeOfParallelism,
        CancellationToken cancellationToken)
    {
        var perRow = new List<ValidationIssue>?[batch.Count];
        Parallel.For(
            0,
            batch.Count,
            new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism, CancellationToken = cancellationToken },
            index => perRow[index] = RowValidator.Validate(batch[index].RowNumber, batch[index].Values, rowRules));

        // Flatten in row order so issues are written in primary-key order.
        var issues = new List<ValidationIssue>();
        foreach (var rowIssues in perRow)
        {
            if (rowIssues is not null)
            {
                issues.AddRange(rowIssues);
            }
        }

        return issues;
    }

    private async IAsyncEnumerable<IReadOnlyList<DatasetRowDto>> ReadBatchesAsync(
        Guid datasetId,
        long afterRowNumber,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cursor = afterRowNumber;
        while (true)
        {
            var batch = await rowReader.ReadBatchAsync(datasetId, cursor, batchSize, cancellationToken);
            if (batch.Count == 0)
            {
                yield break;
            }

            yield return batch;
            cursor = batch[^1].RowNumber;

            if (batch.Count < batchSize)
            {
                yield break;
            }
        }
    }

    private async Task RunUniqueRuleAsync(long runId, Guid datasetId, CompiledRule rule, CancellationToken cancellationToken)
    {
        await issueWriter.DeleteRuleIssuesAsync(runId, rule.RuleId, cancellationToken);

        var buffer = new List<ValidationIssue>(DuplicateIssueBatchSize);
        await foreach (var duplicate in rowReader.ReadDuplicatesAsync(datasetId, rule.ColumnIndex, cancellationToken))
        {
            buffer.Add(new ValidationIssue(
                duplicate.RowNumber,
                rule.RuleId,
                rule.ColumnName,
                rule.RuleType,
                "The value appears more than once in the dataset.",
                ValidationIssue.TruncateValue(duplicate.Value)));

            if (buffer.Count == DuplicateIssueBatchSize)
            {
                await issueWriter.WriteAsync(runId, buffer, cancellationToken);
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
        {
            await issueWriter.WriteAsync(runId, buffer, cancellationToken);
        }
    }

    private async Task FailAsync(ValidationRunEntity run, string reason)
    {
        run.Fail(reason, timeProvider.GetUtcNow());
        await runRepository.UpdateAsync(run, CancellationToken.None);
    }
}
