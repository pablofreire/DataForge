using DataForge.BuildingBlocks.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Enums;

namespace DataForge.Modules.Onboarding.Domain.Entities;

/// <summary>
/// A file uploaded to a project. Its rows are staged in <c>onboarding.dataset_rows</c> by the import job.
/// </summary>
public sealed class DatasetEntity : TenantOwnedEntity
{
    public const int MaxColumns = 1000;

    private DatasetEntity()
    {
    }

    public long ProjectId { get; private set; }

    /// <summary>Public id of the owning project (read through a join; not stored on the dataset row).</summary>
    public Guid ProjectPublicId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ContentSha256 { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public string StoragePath { get; private set; } = string.Empty;

    public DatasetStatus Status { get; private set; }

    public long RowCount { get; private set; }

    public string[] Columns { get; private set; } = [];

    public string? FailureReason { get; private set; }

    public DateTimeOffset? ImportedAt { get; private set; }

    public static DatasetEntity Create(
        long tenantId,
        long projectId,
        Guid projectPublicId,
        Guid publicId,
        string fileName,
        string contentSha256,
        long sizeBytes,
        string storagePath,
        DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(projectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentSha256);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeBytes);

        var dataset = new DatasetEntity
        {
            TenantId = tenantId,
            ProjectId = projectId,
            ProjectPublicId = projectPublicId,
            FileName = fileName,
            ContentSha256 = contentSha256,
            SizeBytes = sizeBytes,
            StoragePath = storagePath,
            Status = DatasetStatus.Uploaded,
        };
        dataset.Initialize(now);
        dataset.PublicId = publicId;
        return dataset;
    }

    public void StartImport(DateTimeOffset now)
    {
        if (Status is not (DatasetStatus.Uploaded or DatasetStatus.Importing))
        {
            throw new InvalidOperationException($"A dataset in status {Status} cannot start importing.");
        }

        Status = DatasetStatus.Importing;
        FailureReason = null;
        Touch(now);
    }

    public void DefineColumns(IReadOnlyList<string> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);
        Columns = [.. columns];
    }

    public void CompleteImport(long rowCount, DateTimeOffset now)
    {
        if (Status != DatasetStatus.Importing)
        {
            throw new InvalidOperationException($"A dataset in status {Status} cannot complete an import.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(rowCount);
        Status = DatasetStatus.Imported;
        RowCount = rowCount;
        ImportedAt = now;
        Touch(now);
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = DatasetStatus.Failed;
        FailureReason = reason;
        Touch(now);
    }
}
