using System.Globalization;
using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

/// <summary>Validates dates against an exact format; ISO 8601 (yyyy-MM-dd) when none is configured.</summary>
public sealed class DateRuleStrategy(string? format) : IRuleStrategy
{
    private const string DefaultFormat = "yyyy-MM-dd";

    private readonly string _format = string.IsNullOrWhiteSpace(format) ? DefaultFormat : format;

    public string? Check(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParseExact(value.Trim(), _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            ? null
            : $"The value is not a valid date in the format {_format}.";
    }
}
