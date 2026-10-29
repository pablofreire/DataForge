using DataForge.Modules.Validation.Domain.Engine.Interfaces;

namespace DataForge.Modules.Validation.Domain.Engine.Strategies;

public sealed class RequiredRuleStrategy : IRuleStrategy
{
    public string? Check(string? value) => string.IsNullOrWhiteSpace(value) ? "A value is required." : null;
}
