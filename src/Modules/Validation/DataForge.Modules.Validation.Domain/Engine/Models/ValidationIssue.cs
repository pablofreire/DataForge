using DataForge.Modules.Validation.Domain.Enums;

namespace DataForge.Modules.Validation.Domain.Engine.Models;

/// <summary>
/// One rule violation. <see cref="RowNumber"/> 0 denotes a dataset-level issue (for example, a rule whose
/// column does not exist in the file).
/// </summary>
public readonly record struct ValidationIssue(long RowNumber, long RuleId, string ColumnName, RuleType RuleType, string Message, string? Value)
{
    public const int ValueMaxLength = 200;

    public static string? TruncateValue(string? value) =>
        value is null || value.Length <= ValueMaxLength ? value : value[..ValueMaxLength];
}
