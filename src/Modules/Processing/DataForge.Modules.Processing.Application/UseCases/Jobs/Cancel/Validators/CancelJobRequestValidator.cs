using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Contracts;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Validators;

public sealed class CancelJobRequestValidator : AbstractValidator<CancelJobRequest>
{
    public CancelJobRequestValidator()
    {
        RuleFor(request => request.JobId).NotEmpty();
    }
}
