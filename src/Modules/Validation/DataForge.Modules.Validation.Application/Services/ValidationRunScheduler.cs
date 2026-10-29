using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Processing.Contracts.Dtos;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Validation.Application.Jobs;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Validation.Application.Services;

/// <summary>
/// Creates a validation run and enqueues the job that executes it. Shared by the automatic trigger
/// (after an import) and the manual "Run validation" action.
/// </summary>
public sealed class ValidationRunScheduler(
    IValidationRunRepository runRepository,
    IJobScheduler jobScheduler,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
{
    public async Task<(ValidationRunEntity Run, JobSummaryDto Job)> ScheduleAsync(Guid datasetId, Guid projectId, CancellationToken cancellationToken)
    {
        var run = ValidationRunEntity.Create(tenantContext.TenantId, datasetId, projectId, timeProvider.GetUtcNow());
        await runRepository.AddAsync(run, cancellationToken);

        var job = await jobScheduler.EnqueueAsync(
            ValidationJobTypes.ValidateDataset,
            run.PublicId,
            ValidationJobTypes.ValidateIdempotencyKey(run.PublicId),
            cancellationToken);

        return (run, job);
    }
}
