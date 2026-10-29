using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Domain.Engine.Services;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.UnitTests.Domain.Engine.Services;

public sealed class RuleCompilerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Compile_MatchesColumnsCaseInsensitively()
    {
        var rules = new[] { Rule("EMAIL", RuleType.Email) };

        var compilation = RuleCompiler.Compile(rules, ["id", "email"], TimeSpan.FromMilliseconds(50));

        compilation.Rules.Should().ContainSingle().Which.ColumnIndex.Should().Be(1);
        compilation.MissingColumnIssues.Should().BeEmpty();
    }

    [Fact]
    public void Compile_MissingColumn_BecomesDatasetLevelIssue()
    {
        var rules = new[] { Rule("segment", RuleType.Required) };

        var compilation = RuleCompiler.Compile(rules, ["id", "email"], TimeSpan.FromMilliseconds(50));

        compilation.Rules.Should().BeEmpty();
        compilation.MissingColumnIssues.Should().ContainSingle()
            .Which.Should().Match<ValidationIssue>(issue => issue.RowNumber == 0 && issue.Message.Contains("segment"));
    }

    [Fact]
    public void Compile_SeparatesRowRulesFromSetRules()
    {
        var rules = new[] { Rule("id", RuleType.Unique), Rule("id", RuleType.Required) };

        var compilation = RuleCompiler.Compile(rules, ["id"], TimeSpan.FromMilliseconds(50));

        compilation.RowRules.Should().ContainSingle().Which.RuleType.Should().Be(RuleType.Required);
        compilation.SetRules.Should().ContainSingle().Which.RuleType.Should().Be(RuleType.Unique);
    }

    private static ValidationRuleEntity Rule(string column, RuleType type) =>
        ValidationRuleEntity.Create(1, Guid.NewGuid(), column, type, RuleParameters.None, Now).Value;
}
