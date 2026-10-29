namespace DataForge.Modules.Validation.Application.UseCases.Runs.Shared;

public sealed record RuleIssueStatItem(Guid RuleId, string ColumnName, string RuleType, long IssueCount);
