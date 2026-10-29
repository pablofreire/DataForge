using DataForge.Modules.Validation.Application.UseCases.Rules.Shared;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.List.Contracts;

public sealed record ListValidationRulesResponse(IReadOnlyList<ValidationRuleItem> Rules);
