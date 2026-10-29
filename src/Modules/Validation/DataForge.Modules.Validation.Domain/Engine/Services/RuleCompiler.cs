using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Domain.Entities;

namespace DataForge.Modules.Validation.Domain.Engine.Services;

/// <summary>
/// Binds project rules to the columns of a dataset. Column names match case-insensitively; rules whose
/// column is missing become dataset-level issues instead of silently passing.
/// </summary>
public static class RuleCompiler
{
    public static RuleCompilation Compile(IReadOnlyList<ValidationRuleEntity> rules, IReadOnlyList<string> columns, TimeSpan regexTimeout)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(columns);

        var columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < columns.Count; index++)
        {
            columnIndexes.TryAdd(columns[index].Trim(), index);
        }

        var compiled = new List<CompiledRule>(rules.Count);
        var missing = new List<ValidationIssue>();

        foreach (var rule in rules)
        {
            if (!columnIndexes.TryGetValue(rule.ColumnName, out var columnIndex))
            {
                missing.Add(new ValidationIssue(0, rule.Id, rule.ColumnName, rule.RuleType, $"The column '{rule.ColumnName}' does not exist in the file.", null));
                continue;
            }

            compiled.Add(new CompiledRule(rule.Id, columns[columnIndex], columnIndex, rule.RuleType, RuleStrategyFactory.Create(rule.RuleType, rule.Parameters, regexTimeout)));
        }

        return new RuleCompilation(compiled, missing);
    }
}
