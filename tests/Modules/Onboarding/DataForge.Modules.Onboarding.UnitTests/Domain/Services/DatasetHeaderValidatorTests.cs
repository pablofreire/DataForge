using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Services;

namespace DataForge.Modules.Onboarding.UnitTests.Domain.Services;

public sealed class DatasetHeaderValidatorTests
{
    [Fact]
    public void Validate_WellFormedHeader_ReturnsNull() =>
        DatasetHeaderValidator.Validate(["id", "name", "email"]).Should().BeNull();

    [Fact]
    public void Validate_NoColumns_ReturnsError() =>
        DatasetHeaderValidator.Validate([]).Should().Contain("no header");

    [Fact]
    public void Validate_BlankColumnName_ReportsItsPosition() =>
        DatasetHeaderValidator.Validate(["id", " ", "email"]).Should().Contain("Column 2");

    [Fact]
    public void Validate_DuplicateNamesIgnoringCase_ReturnsError() =>
        DatasetHeaderValidator.Validate(["Email", "id", "email"]).Should().Contain("more than once");

    [Fact]
    public void Validate_TooManyColumns_ReturnsError() =>
        DatasetHeaderValidator.Validate(Enumerable.Range(0, DatasetEntity.MaxColumns + 1).Select(index => $"c{index}").ToArray())
            .Should().Contain("maximum");
}
