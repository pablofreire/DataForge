namespace DataForge.Modules.Validation.Domain.ReadModels;

public sealed record RuleIssueStat(Guid RuleId, string ColumnName, string RuleType, long IssueCount);
