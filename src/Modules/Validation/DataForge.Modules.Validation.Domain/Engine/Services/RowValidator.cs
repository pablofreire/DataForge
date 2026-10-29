using DataForge.Modules.Validation.Domain.Engine.Models;

namespace DataForge.Modules.Validation.Domain.Engine.Services;

/// <summary>
/// Evaluates row-level rules. Stateless and allocation-light: rows without violations allocate nothing.
/// </summary>
public static class RowValidator
{
    public static List<ValidationIssue>? Validate(long rowNumber, IReadOnlyList<string?> values, IReadOnlyList<CompiledRule> rowRules)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(rowRules);

        List<ValidationIssue>? issues = null;
        foreach (var rule in rowRules)
        {
            // Ragged rows (fewer fields than the header) are treated as having blank trailing values.
            var value = rule.ColumnIndex < values.Count ? values[rule.ColumnIndex] : null;
            var message = rule.Strategy!.Check(value);
            if (message is not null)
            {
                issues ??= [];
                issues.Add(new ValidationIssue(rowNumber, rule.RuleId, rule.ColumnName, rule.RuleType, message, ValidationIssue.TruncateValue(value)));
            }
        }

        return issues;
    }
}
