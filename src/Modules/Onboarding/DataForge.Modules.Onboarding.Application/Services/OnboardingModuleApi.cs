using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.Modules.Onboarding.Application.Mappings;
using DataForge.Modules.Onboarding.Contracts.Dtos;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Onboarding.Application.Services;

public sealed class OnboardingModuleApi(
    IProjectRepository projectRepository,
    IDatasetRepository datasetRepository,
    ITenantContext tenantContext) : IOnboardingModuleApi
{
    public async Task<ProjectSummaryDto?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByPublicIdAsync(tenantContext.TenantId, projectId, cancellationToken);
        return project is null ? null : new ProjectSummaryDto(project.PublicId, project.Name);
    }

    public async Task<DatasetSummaryDto?> GetDatasetAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await datasetRepository.GetByPublicIdAsync(tenantContext.TenantId, datasetId, cancellationToken);
        return dataset?.ToSummaryDto();
    }

    public async Task<OnboardingStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        var totals = await datasetRepository.GetTotalsAsync(tenantContext.TenantId, cancellationToken);
        return new OnboardingStatsDto(totals.ProjectCount, totals.DatasetCount, totals.RowCount);
    }
}
