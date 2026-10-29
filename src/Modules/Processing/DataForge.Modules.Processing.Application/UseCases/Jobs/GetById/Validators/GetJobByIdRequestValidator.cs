using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Contracts;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Validators;

public sealed class GetJobByIdRequestValidator : AbstractValidator<GetJobByIdRequest>
{
    public GetJobByIdRequestValidator()
    {
        RuleFor(request => request.JobId).NotEmpty();
    }
}
