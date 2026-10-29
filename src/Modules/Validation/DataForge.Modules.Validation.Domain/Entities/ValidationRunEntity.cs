using DataForge.BuildingBlocks.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;

namespace DataForge.Modules.Validation.Domain.Entities;

/// <summary>
/// One execution of the project's rules against one dataset. Every (re)validation creates a new run,
/// which keeps the history of how data quality evolved as the customer corrected the file.
/// </summary>
public sealed class ValidationRunEntity : TenantOwnedEntity
{
    private ValidationRunEntity()
    {
    }

    public Guid DatasetPublicId { get; private set; }

    public Guid ProjectPublicId { get; private set; }

    public ValidationRunStatus Status { get; private set; }

    public long TotalRows { get; private set; }

    public long ValidatedRows { get; private set; }

    public long InvalidRows { get; private set; }

    public long IssueCount { get; private set; }

    public int RuleCount { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static ValidationRunEntity Create(long tenantId, Guid datasetPublicId, Guid projectPublicId, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);

        var run = new ValidationRunEntity
        {
            TenantId = tenantId,
            DatasetPublicId = datasetPublicId,
            ProjectPublicId = projectPublicId,
            Status = ValidationRunStatus.Queued,
        };
        run.Initialize(now);
        return run;
    }

    public void Start(long totalRows, int ruleCount, DateTimeOffset now)
    {
        if (Status is ValidationRunStatus.Completed or ValidationRunStatus.Failed)
        {
            throw new InvalidOperationException($"A run in status {Status} cannot be started.");
        }

        Status = ValidationRunStatus.Running;
        TotalRows = totalRows;
        RuleCount = ruleCount;
        StartedAt ??= now;
        Touch(now);
    }

    public void RecordProgress(long validatedRows, DateTimeOffset now)
    {
        ValidatedRows = Math.Min(validatedRows, TotalRows);
        Touch(now);
    }

    public void Complete(long invalidRows, long issueCount, DateTimeOffset now)
    {
        if (Status != ValidationRunStatus.Running)
        {
            throw new InvalidOperationException($"A run in status {Status} cannot be completed.");
        }

        Status = ValidationRunStatus.Completed;
        ValidatedRows = TotalRows;
        InvalidRows = invalidRows;
        IssueCount = issueCount;
        CompletedAt = now;
        Touch(now);
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = ValidationRunStatus.Failed;
        FailureReason = reason;
        CompletedAt = now;
        Touch(now);
    }
}
