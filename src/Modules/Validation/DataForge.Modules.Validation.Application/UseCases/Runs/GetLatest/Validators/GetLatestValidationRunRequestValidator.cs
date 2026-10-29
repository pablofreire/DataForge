using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Contracts;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Validators;

public sealed class GetLatestValidationRunRequestValidator : AbstractValidator<GetLatestValidationRunRequest>
{
    public GetLatestValidationRunRequestValidator()
    {
        RuleFor(request => request.DatasetId).NotEmpty();
    }
}
