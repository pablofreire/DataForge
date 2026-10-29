namespace DataForge.Modules.Validation.Application.UseCases.Issues.Shared;

/// <param name="RowNumber">1-based data row; 0 for dataset-level issues.</param>
/// <param name="ColumnName">Column the rule applies to.</param>
/// <param name="RuleType">Rule type name.</param>
/// <param name="Message">User-facing description of the violation.</param>
/// <param name="Value">Offending value (truncated), when there is one.</param>
public sealed record ValidationIssueItem(long RowNumber, string ColumnName, string RuleType, string Message, string? Value);
