using DataForge.Modules.Processing.Contracts.Dtos;

namespace DataForge.Modules.Processing.Contracts.Interfaces;

/// <summary>
/// Read access to job state for other modules (for example, to show import progress on a dataset page).
/// All queries are scoped to the current tenant.
/// </summary>
public interface IJobQueryService
{
    Task<JobSummaryDto?> GetLatestForResourceAsync(Guid resourceId, string jobType, CancellationToken cancellationToken);
}
