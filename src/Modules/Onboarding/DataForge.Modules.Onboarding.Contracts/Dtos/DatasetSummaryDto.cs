namespace DataForge.Modules.Onboarding.Contracts.Dtos;

public sealed record DatasetSummaryDto(
    Guid DatasetId,
    Guid ProjectId,
    string FileName,
    string Status,
    long RowCount,
    IReadOnlyList<string> Columns);
