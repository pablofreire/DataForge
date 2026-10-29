namespace DataForge.Modules.Validation.Domain.Engine.Models;

public sealed record RuleCompilation(IReadOnlyList<CompiledRule> Rules, IReadOnlyList<ValidationIssue> MissingColumnIssues)
{
    public IEnumerable<CompiledRule> RowRules => Rules.Where(rule => rule.Strategy is not null);

    public IEnumerable<CompiledRule> SetRules => Rules.Where(rule => rule.Strategy is null);
}
