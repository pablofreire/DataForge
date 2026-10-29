using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Contracts;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Validators;

public sealed class GetProjectByIdRequestValidator : AbstractValidator<GetProjectByIdRequest>
{
    public GetProjectByIdRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
    }
}
