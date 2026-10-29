using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Validators;

namespace DataForge.Modules.Onboarding.UnitTests.UseCases.Datasets.Upload.Validators;

public sealed class UploadDatasetRequestValidatorTests
{
    private readonly UploadDatasetRequestValidator _validator = new();

    [Theory]
    [InlineData("customers.csv")]
    [InlineData("export 2026.TSV")]
    [InlineData("data.txt")]
    public void Validate_AcceptedFileNames_AreValid(string fileName) =>
        _validator.Validate(Request(fileName)).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("payload.exe")]
    [InlineData("../../etc/passwd.csv")]
    [InlineData("report.xlsx")]
    [InlineData("")]
    public void Validate_RejectedFileNames_AreInvalid(string fileName) =>
        _validator.Validate(Request(fileName)).IsValid.Should().BeFalse();

    private static UploadDatasetRequest Request(string fileName) => new(Guid.NewGuid(), fileName, Stream.Null);
}
