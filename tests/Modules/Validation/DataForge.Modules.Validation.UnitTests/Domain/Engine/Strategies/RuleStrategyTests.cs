using DataForge.Modules.Validation.Domain.Engine.Services;
using DataForge.Modules.Validation.Domain.Engine.Strategies;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.UnitTests.Domain.Engine.Strategies;

public sealed class RuleStrategyTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("value", true)]
    public void Required_FlagsBlankValues(string? value, bool valid) =>
        new RequiredRuleStrategy().Check(value).Should().Match(message => (message == null) == valid);

    [Theory]
    [InlineData("abc", true)]
    [InlineData("abcd", false)]
    [InlineData("  abc  ", true)]
    [InlineData(null, true)]
    public void MaxLength_ComparesTrimmedLength(string? value, bool valid) =>
        new MaxLengthRuleStrategy(3).Check(value).Should().Match(message => (message == null) == valid);

    [Theory]
    [InlineData("ana@example.com", true)]
    [InlineData("ana.example.com", false)]
    [InlineData("ana@example", false)]
    [InlineData("a b@example.com", false)]
    [InlineData("", true)]
    public void Email_ValidatesSyntax(string? value, bool valid) =>
        new EmailRuleStrategy().Check(value).Should().Match(message => (message == null) == valid);

    [Theory]
    [InlineData("10", true)]
    [InlineData("10.50", true)]
    [InlineData("-1", false)]
    [InlineData("100.01", false)]
    [InlineData("1,5", false)]
    [InlineData("abc", false)]
    public void Numeric_ParsesInvariantNumbersWithinRange(string value, bool valid) =>
        new NumericRuleStrategy(0, 100).Check(value).Should().Match(message => (message == null) == valid);

    [Theory]
    [InlineData("2024-02-29", null, true)]
    [InlineData("2023-02-29", null, false)]
    [InlineData("2024-02-30", null, false)]
    [InlineData("15/05/2024", "dd/MM/yyyy", true)]
    [InlineData("2024-05-15", "dd/MM/yyyy", false)]
    public void Date_ValidatesCalendarAndFormat(string value, string? format, bool valid) =>
        new DateRuleStrategy(format).Check(value).Should().Match(message => (message == null) == valid);

    [Theory]
    [InlineData("br", true)]
    [InlineData(" PT ", true)]
    [InlineData("XX", false)]
    public void AllowedValues_IsCaseInsensitive(string value, bool valid) =>
        new AllowedValuesRuleStrategy(["BR", "PT", "US"]).Check(value).Should().Match(message => (message == null) == valid);

    [Theory]
    [InlineData("AB123456", true)]
    [InlineData("ab123456", false)]
    [InlineData("AB12345", false)]
    public void Pattern_MatchesRegularExpression(string value, bool valid) =>
        new PatternRuleStrategy("^[A-Z]{2}[0-9]{6}$", TimeSpan.FromMilliseconds(100)).Check(value).Should().Match(message => (message == null) == valid);

    [Fact]
    public void Pattern_CatastrophicBacktracking_TimesOutInsteadOfHangingTheWorker()
    {
        var strategy = new PatternRuleStrategy("^(a+)+$", TimeSpan.FromMilliseconds(20));

        var message = strategy.Check(new string('a', 5_000) + "!");

        message.Should().NotBeNull().And.Contain("in time");
    }

    [Fact]
    public void Factory_UniqueRule_IsSetBasedAndHasNoRowStrategy() =>
        RuleStrategyFactory.Create(RuleType.Unique, RuleParameters.None, TimeSpan.FromMilliseconds(50)).Should().BeNull();

    [Theory]
    [InlineData(RuleType.Required)]
    [InlineData(RuleType.Email)]
    [InlineData(RuleType.Date)]
    public void Factory_RowRules_HaveStrategy(RuleType ruleType) =>
        RuleStrategyFactory.Create(ruleType, RuleParameters.None, TimeSpan.FromMilliseconds(50)).Should().NotBeNull();
}
