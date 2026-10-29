namespace DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;

public sealed record ListValidationIssuesRequest(Guid DatasetId, string? Cursor, int Limit, string? ColumnName);
