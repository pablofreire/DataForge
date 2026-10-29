namespace DataForge.Modules.Validation.Application.UseCases.Rules.Shared;

public sealed record ValidationRuleItem(Guid RuleId, string ColumnName, string RuleType, string Description, DateTimeOffset CreatedAt);
