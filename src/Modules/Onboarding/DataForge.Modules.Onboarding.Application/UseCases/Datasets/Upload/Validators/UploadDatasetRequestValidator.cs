using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Validators;

public sealed class UploadDatasetRequestValidator : AbstractValidator<UploadDatasetRequest>
{
    private static readonly string[] AllowedExtensions = [".csv", ".txt", ".tsv"];

    public UploadDatasetRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
        RuleFor(request => request.Content).NotNull();
        RuleFor(request => request.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(name => name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !name.Contains("..", StringComparison.Ordinal))
            .WithMessage("The file name contains invalid characters.")
            .Must(name => AllowedExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Only .csv, .tsv and .txt files are accepted.");
    }
}
