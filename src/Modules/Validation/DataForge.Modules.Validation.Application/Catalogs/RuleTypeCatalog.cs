using DataForge.Modules.Validation.Domain.Enums;

namespace DataForge.Modules.Validation.Application.Catalogs;

/// <summary>Rule types offered to clients, with user-facing labels (keeps the presentation layer off the domain).</summary>
public static class RuleTypeCatalog
{
    public static IReadOnlyList<(string Value, string Label)> All { get; } =
    [
        (nameof(RuleType.Required), "Required"),
        (nameof(RuleType.MaxLength), "Maximum length"),
        (nameof(RuleType.Pattern), "Matches pattern"),
        (nameof(RuleType.Email), "E-mail address"),
        (nameof(RuleType.Numeric), "Number"),
        (nameof(RuleType.Date), "Date"),
        (nameof(RuleType.AllowedValues), "One of a list"),
        (nameof(RuleType.Unique), "Unique in file"),
    ];
}
