using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Processing.Application.Mappings;
using DataForge.Modules.Processing.Application.Options;
using DataForge.Modules.Processing.Contracts.Dtos;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Domain.Entities;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Processing.Application.Services;

public sealed class JobScheduler(
    IJobRepository jobRepository,
    ITenantContext tenantContext,
    IOptions<ProcessingOptions> options,
    TimeProvider timeProvider) : IJobScheduler
{
    public async Task<JobSummaryDto> EnqueueAsync(string jobType, Guid resourceId, string idempotencyKey, CancellationToken cancellationToken)
    {
        var job = JobEntity.Create(
            tenantContext.TenantId,
            tenantContext.TenantPublicId,
            jobType,
            resourceId,
            idempotencyKey,
            options.Value.MaxAttempts,
            timeProvider.GetUtcNow());

        var stored = await jobRepository.EnqueueAsync(job, cancellationToken);
        return stored.ToSummaryDto();
    }
}
