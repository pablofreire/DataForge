using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.ValueObjects;

namespace DataForge.Modules.Validation.UnitTests.Domain.Entities;

public sealed class ValidationRuleEntityTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_MaxLengthWithoutLimit_Fails() =>
        Create(RuleType.MaxLength, new RuleParameters()).IsFailure.Should().BeTrue();

    [Fact]
    public void Create_InvalidRegex_Fails() =>
        Create(RuleType.Pattern, new RuleParameters { Pattern = "([a-z" }).Error.Code.Should().Be("rule.invalid_parameters");

    [Fact]
    public void Create_NumericWithMinAboveMax_Fails() =>
        Create(RuleType.Numeric, new RuleParameters { Min = 10, Max = 1 }).IsFailure.Should().BeTrue();

    [Fact]
    public void Create_AllowedValues_AreTrimmedAndDeduplicated()
    {
        var result = Create(RuleType.AllowedValues, new RuleParameters { AllowedValues = [" BR ", "br", "PT", ""] });

        result.IsSuccess.Should().BeTrue();
        result.Value.Parameters.AllowedValues.Should().Equal("BR", "PT");
    }

    [Fact]
    public void Create_KeepsOnlyParametersRelevantToTheRuleType()
    {
        var result = Create(RuleType.Required, new RuleParameters { MaxLength = 10, Pattern = ".*" });

        result.Value.Parameters.Should().Be(RuleParameters.None);
    }

    [Fact]
    public void Create_BlankColumn_Fails() =>
        ValidationRuleEntity.Create(1, Guid.NewGuid(), " ", RuleType.Required, null, Now).IsFailure.Should().BeTrue();

    private static DataForge.BuildingBlocks.Domain.Results.Result<ValidationRuleEntity> Create(RuleType type, RuleParameters parameters) =>
        ValidationRuleEntity.Create(1, Guid.NewGuid(), "column", type, parameters, Now);
}
