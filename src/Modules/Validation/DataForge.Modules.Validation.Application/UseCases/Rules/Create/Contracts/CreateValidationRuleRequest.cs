namespace DataForge.Modules.Validation.Application.UseCases.Rules.Create.Contracts;

public sealed record CreateValidationRuleRequest(Guid ProjectId, string ColumnName, string RuleType, int? MaxLength = null, string? Pattern = null, decimal? Min = null, decimal? Max = null, string? Format = null, IReadOnlyList<string>? AllowedValues = null);
