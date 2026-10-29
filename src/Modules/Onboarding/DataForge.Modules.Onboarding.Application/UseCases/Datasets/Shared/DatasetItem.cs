namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.Shared;

public sealed record DatasetItem(
    Guid DatasetId,
    Guid ProjectId,
    string FileName,
    long SizeBytes,
    string Status,
    long RowCount,
    IReadOnlyList<string> Columns,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ImportedAt);
