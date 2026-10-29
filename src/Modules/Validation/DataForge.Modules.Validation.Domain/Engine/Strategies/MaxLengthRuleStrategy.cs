using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

/// <summary>Blank values pass; combine with <see cref="RequiredRuleStrategy"/> to forbid them.</summary>
public sealed class MaxLengthRuleStrategy(int maxLength) : IRuleStrategy
{
    private readonly string _message = $"The value exceeds {maxLength} characters.";

    public string? Check(string? value) => value is not null && value.Trim().Length > maxLength ? _message : null;
}
