namespace DataForge.Modules.Validation.Presentation.Models;

/// <summary>Form state for a new rule. Parameter fields are shown or hidden by rule type.</summary>
public sealed class RuleFormModel
{
    public string ColumnName { get; set; } = string.Empty;

    public string RuleType { get; set; } = "Required";

    public int? MaxLength { get; set; }

    public string? Pattern { get; set; }

    public decimal? Min { get; set; }

    public decimal? Max { get; set; }

    public string? Format { get; set; }

    /// <summary>Comma-separated list, split on submit.</summary>
    public string? AllowedValues { get; set; }
}
