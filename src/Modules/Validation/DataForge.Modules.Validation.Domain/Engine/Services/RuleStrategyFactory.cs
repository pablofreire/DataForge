using DataForge.Modules.Validation.Domain.Engine.Interfaces;
using DataForge.Modules.Validation.Domain.Engine.Strategies;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.Domain.Engine.Services;

/// <summary>
/// Creates the row-level strategy for a rule. Returns null for set-based rules (<see cref="RuleType.Unique"/>),
/// which are evaluated by the database instead of row by row.
/// </summary>
public static class RuleStrategyFactory
{
    public static IRuleStrategy? Create(RuleType ruleType, RuleParameters parameters, TimeSpan regexTimeout)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return ruleType switch
        {
            RuleType.Required => new RequiredRuleStrategy(),
            RuleType.MaxLength => new MaxLengthRuleStrategy(parameters.MaxLength ?? int.MaxValue),
            RuleType.Pattern => new PatternRuleStrategy(parameters.Pattern ?? ".*", regexTimeout),
            RuleType.Email => new EmailRuleStrategy(),
            RuleType.Numeric => new NumericRuleStrategy(parameters.Min, parameters.Max),
            RuleType.Date => new DateRuleStrategy(parameters.Format),
            RuleType.AllowedValues => new AllowedValuesRuleStrategy(parameters.AllowedValues ?? []),
            RuleType.Unique => null,
            _ => throw new ArgumentOutOfRangeException(nameof(ruleType), ruleType, "Unknown rule type."),
        };
    }
}
