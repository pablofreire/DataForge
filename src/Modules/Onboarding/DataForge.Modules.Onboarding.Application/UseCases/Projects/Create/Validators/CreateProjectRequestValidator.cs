using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;
using DataForge.Modules.Onboarding.Domain.Entities;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Validators;

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(ProjectEntity.NameMaxLength);
        RuleFor(request => request.Description).MaximumLength(ProjectEntity.DescriptionMaxLength);
    }
}
