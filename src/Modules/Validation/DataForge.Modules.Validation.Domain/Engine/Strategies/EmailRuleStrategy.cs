using System.Text.RegularExpressions;
using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

/// <summary>Pragmatic syntax check (local@domain.tld); deliverability is out of scope.</summary>
public sealed partial class EmailRuleStrategy : IRuleStrategy
{
    public string? Check(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 254 && EmailRegex().IsMatch(trimmed) ? null : "The value is not a valid e-mail address.";
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex EmailRegex();
}
