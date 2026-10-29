using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Contracts;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Validators;

public sealed class ListProjectDatasetsRequestValidator : AbstractValidator<ListProjectDatasetsRequest>
{
    public ListProjectDatasetsRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
    }
}
