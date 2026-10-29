using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Contracts;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.Start.Validators;

public sealed class StartValidationRunRequestValidator : AbstractValidator<StartValidationRunRequest>
{
    public StartValidationRunRequestValidator()
    {
        RuleFor(request => request.DatasetId).NotEmpty();
    }
}
