using System.Collections.Frozen;
using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

public sealed class AllowedValuesRuleStrategy(IEnumerable<string> allowedValues) : IRuleStrategy
{
    private readonly FrozenSet<string> _allowed = allowedValues.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string? Check(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _allowed.Contains(value.Trim()) ? null : "The value is not one of the allowed values.";
    }
}
