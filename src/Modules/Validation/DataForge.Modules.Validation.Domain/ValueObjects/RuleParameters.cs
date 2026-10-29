namespace DataForge.Modules.Validation.Domain.ValueObjects;

/// <summary>
/// Parameters of a rule. Only the members relevant to the rule type are set; stored as JSON.
/// </summary>
public sealed record RuleParameters
{
    public static readonly RuleParameters None = new();

    public int? MaxLength { get; init; }

    public string? Pattern { get; init; }

    public decimal? Min { get; init; }

    public decimal? Max { get; init; }

    public string? Format { get; init; }

    public IReadOnlyList<string>? AllowedValues { get; init; }
}
