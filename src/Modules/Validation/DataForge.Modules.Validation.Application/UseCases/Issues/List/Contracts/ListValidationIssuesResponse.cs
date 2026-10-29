using DataForge.Modules.Validation.Application.UseCases.Issues.Shared;

namespace DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;

public sealed record ListValidationIssuesResponse(Guid? RunId, IReadOnlyList<ValidationIssueItem> Issues, string? NextCursor);
