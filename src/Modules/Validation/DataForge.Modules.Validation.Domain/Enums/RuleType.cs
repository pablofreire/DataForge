using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.Domain.Enums;

public enum RuleType
{
    /// <summary>The value must not be blank.</summary>
    Required = 0,

    /// <summary>The value must not exceed <see cref="RuleParameters.MaxLength"/> characters.</summary>
    MaxLength = 1,

    /// <summary>The value must match the regular expression in <see cref="RuleParameters.Pattern"/>.</summary>
    Pattern = 2,

    /// <summary>The value must be a syntactically valid e-mail address.</summary>
    Email = 3,

    /// <summary>The value must be a number, optionally within <see cref="RuleParameters.Min"/>..<see cref="RuleParameters.Max"/>.</summary>
    Numeric = 4,

    /// <summary>The value must be a date in <see cref="RuleParameters.Format"/> (ISO 8601 by default).</summary>
    Date = 5,

    /// <summary>The value must be one of <see cref="RuleParameters.AllowedValues"/> (case-insensitive).</summary>
    AllowedValues = 6,

    /// <summary>The value must be unique within the dataset. Evaluated set-based in SQL, not row by row.</summary>
    Unique = 7,
}
