using DataForge.BuildingBlocks.Domain.Entities;
using DataForge.Modules.Processing.Domain.Enums;

namespace DataForge.Modules.Processing.Domain.Entities;

/// <summary>
/// A unit of background work stored in a durable PostgreSQL queue.
/// <para>
/// Ownership is lease-based: a worker claims a job for <c>LeaseExpiresAt</c> and renews the lease while
/// it runs. If the worker dies, the lease expires and another worker resumes from <see cref="CheckpointPosition"/>.
/// All state-changing statements are fenced by <see cref="LeaseOwner"/>, so a worker that lost its lease
/// can never overwrite the progress of the new owner.
/// </para>
/// </summary>
public sealed class JobEntity : TenantOwnedEntity
{
    private JobEntity()
    {
    }

    public Guid TenantPublicId { get; private set; }

    public string JobType { get; private set; } = string.Empty;

    public Guid ResourceId { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public JobStatus Status { get; private set; }

    public int Attempt { get; private set; }

    public int MaxAttempts { get; private set; }

    public long ProcessedItems { get; private set; }

    public long? TotalItems { get; private set; }

    public long CheckpointPosition { get; private set; }

    public string? LeaseOwner { get; private set; }

    public DateTimeOffset? LeaseExpiresAt { get; private set; }

    public DateTimeOffset RunAfter { get; private set; }

    public bool CancelRequested { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public bool IsTerminal => Status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled;

    public bool CanBeCancelled => Status is JobStatus.Queued or JobStatus.Running;

    public bool HasAttemptsLeft => Attempt < MaxAttempts;

    public static JobEntity Create(
        long tenantId,
        Guid tenantPublicId,
        string jobType,
        Guid resourceId,
        string idempotencyKey,
        int maxAttempts,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(jobType);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);

        var job = new JobEntity
        {
            TenantId = tenantId,
            TenantPublicId = tenantPublicId,
            JobType = jobType,
            ResourceId = resourceId,
            IdempotencyKey = idempotencyKey,
            Status = JobStatus.Queued,
            MaxAttempts = maxAttempts,
            RunAfter = now,
        };
        job.Initialize(now);
        return job;
    }
}
