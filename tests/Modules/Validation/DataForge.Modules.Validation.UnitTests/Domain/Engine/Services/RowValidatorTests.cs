using DataForge.Modules.Validation.Domain.Engine.Models;
using DataForge.Modules.Validation.Domain.Engine.Services;
using DataForge.Modules.Validation.Domain.Engine.Strategies;
using DataForge.Modules.Validation.Domain.Enums;

namespace DataForge.Modules.Validation.UnitTests.Domain.Engine.Services;

public sealed class RowValidatorTests
{
    private static readonly CompiledRule[] Rules =
    [
        new(1, "name", 0, RuleType.Required, new RequiredRuleStrategy()),
        new(2, "email", 1, RuleType.Email, new EmailRuleStrategy()),
    ];

    [Fact]
    public void Validate_ValidRow_ReturnsNullWithoutAllocatingIssues() =>
        RowValidator.Validate(1, ["Alice", "alice@example.com"], Rules).Should().BeNull();

    [Fact]
    public void Validate_EveryViolatedRuleProducesOneIssue()
    {
        var issues = RowValidator.Validate(7, ["", "not-an-email"], Rules);

        issues.Should().HaveCount(2);
        issues!.Select(issue => issue.RuleId).Should().Equal(1, 2);
        issues.Should().OnlyContain(issue => issue.RowNumber == 7);
        issues[1].Value.Should().Be("not-an-email");
    }

    [Fact]
    public void Validate_RaggedRow_MissingTrailingValuesAreBlank()
    {
        var issues = RowValidator.Validate(3, ["Alice"], Rules);

        issues.Should().BeNull("a missing optional value does not break the e-mail rule");
        RowValidator.Validate(3, [], Rules).Should().ContainSingle().Which.RuleId.Should().Be(1);
    }

    [Fact]
    public void Validate_LongValue_IsTruncatedOnTheIssue()
    {
        var longValue = new string('x', 500);

        var issues = RowValidator.Validate(1, ["Alice", longValue], Rules);

        issues.Should().ContainSingle().Which.Value.Should().HaveLength(ValidationIssue.ValueMaxLength);
    }
}
