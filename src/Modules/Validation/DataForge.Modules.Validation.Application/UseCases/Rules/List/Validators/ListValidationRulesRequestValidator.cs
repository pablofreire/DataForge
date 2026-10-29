using DataForge.Modules.Validation.Application.UseCases.Rules.List.Contracts;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.List.Validators;

public sealed class ListValidationRulesRequestValidator : AbstractValidator<ListValidationRulesRequest>
{
    public ListValidationRulesRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
    }
}
