namespace DataForge.Modules.Processing.Contracts.Dtos;

public sealed record JobSummaryDto(
    Guid JobId,
    string JobType,
    Guid ResourceId,
    string Status,
    int Attempt,
    int MaxAttempts,
    long ProcessedItems,
    long? TotalItems,
    string? LastError,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt)
{
    public bool IsTerminal => Status is "Succeeded" or "Failed" or "Cancelled";

    public double? ProgressPercent => TotalItems is > 0
        ? Math.Min(100d, ProcessedItems * 100d / TotalItems.Value)
        : Status == "Succeeded" ? 100d : null;
}
