namespace DataForge.Modules.Processing.Infrastructure.Persistence.Queries;

/// <summary>
/// SQL for the job queue. Lease timestamps use the database clock (<c>now()</c>) so that every worker
/// agrees on lease expiry regardless of clock skew between machines.
/// </summary>
internal static class JobQueries
{
    private const string Columns = """
        id, public_id, tenant_id, tenant_public_id, job_type, resource_id, idempotency_key, status,
        attempt, max_attempts, processed_items, total_items, checkpoint_position, lease_owner,
        lease_expires_at, run_after, cancel_requested, last_error, started_at, completed_at,
        is_active, created_at, updated_at
        """;

    public const string Enqueue = $"""
        INSERT INTO processing.jobs (
            public_id, tenant_id, tenant_public_id, job_type, resource_id, idempotency_key, status,
            max_attempts, run_after, created_at, updated_at)
        VALUES (
            @PublicId, @TenantId, @TenantPublicId, @JobType, @ResourceId, @IdempotencyKey, 'Queued',
            @MaxAttempts, now(), @CreatedAt, @UpdatedAt)
        ON CONFLICT (idempotency_key) DO NOTHING
        RETURNING {Columns};
        """;

    public const string GetByIdempotencyKey = $"""
        SELECT {Columns} FROM processing.jobs WHERE idempotency_key = @IdempotencyKey;
        """;

    public const string ClaimNext = """
        WITH next_job AS (
            SELECT id
            FROM processing.jobs
            WHERE (status = 'Queued' AND run_after <= now())
               OR (status = 'Running' AND lease_expires_at < now() AND attempt < max_attempts)
            ORDER BY run_after, id
            FOR UPDATE SKIP LOCKED
            LIMIT 1
        )
        UPDATE processing.jobs AS j
        SET status = 'Running',
            lease_owner = @WorkerId,
            lease_expires_at = now() + make_interval(secs => @LeaseSeconds),
            attempt = j.attempt + 1,
            started_at = COALESCE(j.started_at, now()),
            last_error = NULL,
            updated_at = now()
        FROM next_job
        WHERE j.id = next_job.id
        RETURNING j.id, j.public_id, j.tenant_id, j.tenant_public_id, j.job_type, j.resource_id,
                  j.idempotency_key, j.status, j.attempt, j.max_attempts, j.processed_items, j.total_items,
                  j.checkpoint_position, j.lease_owner, j.lease_expires_at, j.run_after, j.cancel_requested,
                  j.last_error, j.started_at, j.completed_at, j.is_active, j.created_at, j.updated_at;
        """;

    /// <summary>Running jobs whose worker died and that have no attempts left are failed instead of being retried forever.</summary>
    public const string FailAbandoned = """
        UPDATE processing.jobs
        SET status = 'Failed',
            last_error = 'The worker stopped responding and no attempts are left.',
            lease_owner = NULL,
            lease_expires_at = NULL,
            completed_at = now(),
            updated_at = now()
        WHERE status = 'Running' AND lease_expires_at < now() AND attempt >= max_attempts;
        """;

    public const string RenewLease = """
        UPDATE processing.jobs
        SET lease_expires_at = now() + make_interval(secs => @LeaseSeconds),
            updated_at = now()
        WHERE id = @JobId AND lease_owner = @WorkerId AND status = 'Running'
        RETURNING cancel_requested;
        """;

    public const string SaveProgress = """
        UPDATE processing.jobs
        SET processed_items = @ProcessedItems,
            total_items = @TotalItems,
            checkpoint_position = @CheckpointPosition,
            updated_at = now()
        WHERE id = @JobId AND lease_owner = @WorkerId AND status = 'Running';
        """;

    public const string MarkSucceeded = """
        UPDATE processing.jobs
        SET status = 'Succeeded', lease_owner = NULL, lease_expires_at = NULL, completed_at = now(), updated_at = now()
        WHERE id = @JobId AND lease_owner = @WorkerId;
        """;

    public const string ScheduleRetry = """
        UPDATE processing.jobs
        SET status = 'Queued', lease_owner = NULL, lease_expires_at = NULL, run_after = now() + make_interval(secs => @DelaySeconds),
            last_error = @Error, updated_at = now()
        WHERE id = @JobId AND lease_owner = @WorkerId;
        """;

    public const string MarkFailed = """
        UPDATE processing.jobs
        SET status = 'Failed', lease_owner = NULL, lease_expires_at = NULL, last_error = @Error,
            completed_at = now(), updated_at = now()
        WHERE id = @JobId AND (lease_owner = @WorkerId OR lease_owner IS NULL);
        """;

    public const string MarkCancelled = """
        UPDATE processing.jobs
        SET status = 'Cancelled', lease_owner = NULL, lease_expires_at = NULL, completed_at = now(), updated_at = now()
        WHERE id = @JobId AND lease_owner = @WorkerId;
        """;

    /// <summary>Graceful shutdown: give the job back without consuming an attempt.</summary>
    public const string Release = """
        UPDATE processing.jobs
        SET status = 'Queued', lease_owner = NULL, lease_expires_at = NULL, run_after = now(),
            attempt = GREATEST(attempt - 1, 0), updated_at = now()
        WHERE id = @JobId AND lease_owner = @WorkerId;
        """;

    public const string GetByPublicId = $"""
        SELECT {Columns} FROM processing.jobs
        WHERE tenant_id = @TenantId AND public_id = @PublicId AND is_active = true;
        """;

    public const string GetLatestForResource = $"""
        SELECT {Columns} FROM processing.jobs
        WHERE tenant_id = @TenantId AND resource_id = @ResourceId AND job_type = @JobType AND is_active = true
        ORDER BY id DESC
        LIMIT 1;
        """;

    public const string ListRecentForTenant = $"""
        SELECT {Columns} FROM processing.jobs
        WHERE tenant_id = @TenantId AND is_active = true
        ORDER BY id DESC
        LIMIT @Limit;
        """;

    public const string ListRecentAllTenants = $"""
        SELECT {Columns} FROM processing.jobs
        WHERE is_active = true
        ORDER BY id DESC
        LIMIT @Limit;
        """;

    public const string RequestCancel = """
        UPDATE processing.jobs
        SET status = CASE WHEN status = 'Queued' THEN 'Cancelled' ELSE status END,
            completed_at = CASE WHEN status = 'Queued' THEN now() ELSE completed_at END,
            cancel_requested = true,
            updated_at = now()
        WHERE tenant_id = @TenantId AND public_id = @PublicId AND status IN ('Queued', 'Running');
        """;

    public const string CountRunnable = """
        SELECT count(*) FROM processing.jobs WHERE status = 'Queued' AND run_after <= now();
        """;
}
