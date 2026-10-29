using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Processing.Application.Mappings;
using DataForge.Modules.Processing.Contracts.Dtos;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Processing.Application.Services;

public sealed class JobQueryService(IJobRepository jobRepository, ITenantContext tenantContext) : IJobQueryService
{
    public async Task<JobSummaryDto?> GetLatestForResourceAsync(Guid resourceId, string jobType, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetLatestForResourceAsync(tenantContext.TenantId, resourceId, jobType, cancellationToken);
        return job?.ToSummaryDto();
    }
}
