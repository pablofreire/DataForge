namespace DataForge.Modules.Validation.Application.UseCases.Runs.Shared;

public sealed record ValidationRunItem(
    Guid RunId,
    Guid DatasetId,
    string Status,
    long TotalRows,
    long ValidatedRows,
    long InvalidRows,
    long ValidRows,
    long IssueCount,
    int RuleCount,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
