using DataForge.Modules.Onboarding.Contracts.Dtos;

namespace DataForge.Modules.Onboarding.Contracts.Interfaces;

/// <summary>
/// Public API of the Onboarding module for other modules. All calls are scoped to the current tenant,
/// so a module can never read another tenant's projects or datasets through it.
/// </summary>
public interface IOnboardingModuleApi
{
    Task<ProjectSummaryDto?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken);

    Task<DatasetSummaryDto?> GetDatasetAsync(Guid datasetId, CancellationToken cancellationToken);

    Task<OnboardingStatsDto> GetStatsAsync(CancellationToken cancellationToken);
}
