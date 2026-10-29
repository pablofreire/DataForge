using System.Globalization;
using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

/// <summary>
/// Accepts invariant-culture numbers (dot as decimal separator, no thousands separators), optionally within a range.
/// Thousands separators are rejected on purpose: "1,5" in a Brazilian export means 1.5, and silently reading it as 15
/// would corrupt the data being onboarded.
/// </summary>
public sealed class NumericRuleStrategy(decimal? min, decimal? max) : IRuleStrategy
{
    private const NumberStyles Styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public string? Check(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!decimal.TryParse(value.Trim(), Styles, CultureInfo.InvariantCulture, out var number))
        {
            return "The value is not a valid number.";
        }

        if (min is not null && number < min)
        {
            return string.Create(CultureInfo.InvariantCulture, $"The value must be greater than or equal to {min}.");
        }

        if (max is not null && number > max)
        {
            return string.Create(CultureInfo.InvariantCulture, $"The value must be less than or equal to {max}.");
        }

        return null;
    }
}
