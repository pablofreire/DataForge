using System.Text.RegularExpressions;
using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

/// <summary>
/// Matches a customer-supplied regular expression. The match timeout protects the workers against
/// catastrophic backtracking (ReDoS) from malicious or careless patterns; a timeout counts as a violation.
/// </summary>
public sealed class PatternRuleStrategy(string pattern, TimeSpan matchTimeout) : IRuleStrategy
{
    private readonly Regex _regex = new(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled, matchTimeout);

    public string? Check(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return _regex.IsMatch(value.Trim()) ? null : "The value does not match the required format.";
        }
        catch (RegexMatchTimeoutException)
        {
            return "The value could not be checked against the pattern in time.";
        }
    }
}
