using DataForge.Modules.Processing.Contracts.Dtos;

namespace DataForge.Modules.Processing.Contracts.Interfaces;

/// <summary>
/// Enqueues background jobs for the current tenant. Enqueueing is idempotent: the same
/// <c>idempotencyKey</c> always returns the job created by the first call.
/// </summary>
public interface IJobScheduler
{
    Task<JobSummaryDto> EnqueueAsync(string jobType, Guid resourceId, string idempotencyKey, CancellationToken cancellationToken);
}
