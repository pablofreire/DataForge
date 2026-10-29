using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Processing.Domain.Entities;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using DataForge.Modules.Processing.Domain.ValueObjects;
using DataForge.Modules.Processing.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Processing.Infrastructure.Persistence.Repositories;

public sealed class JobRepository(IDbConnectionFactory connectionFactory) : IJobRepository
{
    public async Task<JobEntity> EnqueueAsync(JobEntity job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var inserted = await connection.QuerySingleOrDefaultAsync<JobEntity>(new CommandDefinition(
            JobQueries.Enqueue,
            new
            {
                job.PublicId,
                job.TenantId,
                job.TenantPublicId,
                job.JobType,
                job.ResourceId,
                job.IdempotencyKey,
                job.MaxAttempts,
                job.CreatedAt,
                job.UpdatedAt,
            },
            cancellationToken: cancellationToken));

        return inserted ?? await connection.QuerySingleAsync<JobEntity>(new CommandDefinition(
            JobQueries.GetByIdempotencyKey,
            new { job.IdempotencyKey },
            cancellationToken: cancellationToken));
    }

    public async Task<JobEntity?> TryClaimNextAsync(string workerId, TimeSpan leaseDuration, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<JobEntity>(new CommandDefinition(
            JobQueries.ClaimNext,
            new { WorkerId = workerId, LeaseSeconds = leaseDuration.TotalSeconds },
            cancellationToken: cancellationToken));
    }

    public async Task<int> FailAbandonedAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition(JobQueries.FailAbandoned, cancellationToken: cancellationToken));
    }

    public async Task<JobLeaseState> RenewLeaseAsync(long jobId, string workerId, TimeSpan leaseDuration, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var cancelRequested = await connection.QuerySingleOrDefaultAsync<bool?>(new CommandDefinition(
            JobQueries.RenewLease,
            new { JobId = jobId, WorkerId = workerId, LeaseSeconds = leaseDuration.TotalSeconds },
            cancellationToken: cancellationToken));

        return cancelRequested is null
            ? new JobLeaseState(IsOwned: false, CancelRequested: false)
            : new JobLeaseState(IsOwned: true, CancelRequested: cancelRequested.Value);
    }

    public async Task<bool> SaveProgressAsync(long jobId, string workerId, long processedItems, long? totalItems, long checkpointPosition, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            JobQueries.SaveProgress,
            new { JobId = jobId, WorkerId = workerId, ProcessedItems = processedItems, TotalItems = totalItems, CheckpointPosition = checkpointPosition },
            cancellationToken: cancellationToken));
        return affected == 1;
    }

    public Task MarkSucceededAsync(long jobId, string workerId, CancellationToken cancellationToken) =>
        ExecuteAsync(JobQueries.MarkSucceeded, new { JobId = jobId, WorkerId = workerId }, cancellationToken);

    public Task ScheduleRetryAsync(long jobId, string workerId, TimeSpan delay, string error, CancellationToken cancellationToken) =>
        ExecuteAsync(JobQueries.ScheduleRetry, new { JobId = jobId, WorkerId = workerId, DelaySeconds = delay.TotalSeconds, Error = Truncate(error) }, cancellationToken);

    public Task MarkFailedAsync(long jobId, string workerId, string error, CancellationToken cancellationToken) =>
        ExecuteAsync(JobQueries.MarkFailed, new { JobId = jobId, WorkerId = workerId, Error = Truncate(error) }, cancellationToken);

    public Task MarkCancelledAsync(long jobId, string workerId, CancellationToken cancellationToken) =>
        ExecuteAsync(JobQueries.MarkCancelled, new { JobId = jobId, WorkerId = workerId }, cancellationToken);

    public Task ReleaseAsync(long jobId, string workerId, CancellationToken cancellationToken) =>
        ExecuteAsync(JobQueries.Release, new { JobId = jobId, WorkerId = workerId }, cancellationToken);

    public async Task<JobEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<JobEntity>(new CommandDefinition(
            JobQueries.GetByPublicId,
            new { TenantId = tenantId, PublicId = publicId },
            cancellationToken: cancellationToken));
    }

    public async Task<JobEntity?> GetLatestForResourceAsync(long tenantId, Guid resourceId, string jobType, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<JobEntity>(new CommandDefinition(
            JobQueries.GetLatestForResource,
            new { TenantId = tenantId, ResourceId = resourceId, JobType = jobType },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<JobEntity>> ListRecentAsync(long? tenantId, int limit, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var command = tenantId is null
            ? new CommandDefinition(JobQueries.ListRecentAllTenants, new { Limit = limit }, cancellationToken: cancellationToken)
            : new CommandDefinition(JobQueries.ListRecentForTenant, new { TenantId = tenantId.Value, Limit = limit }, cancellationToken: cancellationToken);

        var jobs = await connection.QueryAsync<JobEntity>(command);
        return jobs.AsList();
    }

    public async Task<bool> RequestCancelAsync(long tenantId, Guid publicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            JobQueries.RequestCancel,
            new { TenantId = tenantId, PublicId = publicId },
            cancellationToken: cancellationToken));
        return affected == 1;
    }

    public async Task<long> CountRunnableAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(JobQueries.CountRunnable, cancellationToken: cancellationToken));
    }

    private async Task ExecuteAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    private static string Truncate(string value) => value.Length <= 2000 ? value : value[..2000];
}
