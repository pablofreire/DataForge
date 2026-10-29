using DataForge.Modules.Validation.Domain.Engine.Interfaces;
using DataForge.Modules.Validation.Domain.Enums;

namespace DataForge.Modules.Validation.Domain.Engine.Models;

/// <summary>A rule bound to a column position of a specific dataset, ready to evaluate.</summary>
/// <param name="RuleId">Internal rule key, stored on each issue.</param>
/// <param name="ColumnName">Column name as it appears in the dataset header.</param>
/// <param name="ColumnIndex">0-based position of the column in the dataset.</param>
/// <param name="RuleType">The rule type.</param>
/// <param name="Strategy">Row-level strategy; null for set-based rules.</param>
public sealed record CompiledRule(long RuleId, string ColumnName, int ColumnIndex, RuleType RuleType, IRuleStrategy? Strategy);
