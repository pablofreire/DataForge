using DataForge.Modules.Processing.Domain.Entities;
using DataForge.Modules.Processing.Domain.ValueObjects;

namespace DataForge.Modules.Processing.Domain.Interfaces.Repositories;

public interface IJobRepository
{
    /// <summary>Inserts the job, or returns the existing job with the same idempotency key.</summary>
    Task<JobEntity> EnqueueAsync(JobEntity job, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically claims the next runnable job (queued and due, or running with an expired lease)
    /// using <c>FOR UPDATE SKIP LOCKED</c>, so concurrent workers never claim the same job.
    /// </summary>
    Task<JobEntity?> TryClaimNextAsync(string workerId, TimeSpan leaseDuration, CancellationToken cancellationToken);

    /// <summary>Fails running jobs whose lease expired after their last allowed attempt. Returns the number of rows affected.</summary>
    Task<int> FailAbandonedAsync(CancellationToken cancellationToken);

    Task<JobLeaseState> RenewLeaseAsync(long jobId, string workerId, TimeSpan leaseDuration, CancellationToken cancellationToken);

    Task<bool> SaveProgressAsync(long jobId, string workerId, long processedItems, long? totalItems, long checkpointPosition, CancellationToken cancellationToken);

    Task MarkSucceededAsync(long jobId, string workerId, CancellationToken cancellationToken);

    /// <summary>Requeues the job to run after <paramref name="delay"/>, measured on the database clock.</summary>
    Task ScheduleRetryAsync(long jobId, string workerId, TimeSpan delay, string error, CancellationToken cancellationToken);

    Task MarkFailedAsync(long jobId, string workerId, string error, CancellationToken cancellationToken);

    Task MarkCancelledAsync(long jobId, string workerId, CancellationToken cancellationToken);

    /// <summary>Returns a running job to the queue (graceful shutdown) while keeping its checkpoint.</summary>
    Task ReleaseAsync(long jobId, string workerId, CancellationToken cancellationToken);

    Task<JobEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken);

    Task<JobEntity?> GetLatestForResourceAsync(long tenantId, Guid resourceId, string jobType, CancellationToken cancellationToken);

    /// <summary>Lists recent jobs. A null <paramref name="tenantId"/> lists all tenants (implementation console only).</summary>
    Task<IReadOnlyList<JobEntity>> ListRecentAsync(long? tenantId, int limit, CancellationToken cancellationToken);

    /// <summary>Cancels a queued job immediately, or flags a running job so its worker stops at the next heartbeat.</summary>
    Task<bool> RequestCancelAsync(long tenantId, Guid publicId, CancellationToken cancellationToken);

    Task<long> CountRunnableAsync(CancellationToken cancellationToken);
}
