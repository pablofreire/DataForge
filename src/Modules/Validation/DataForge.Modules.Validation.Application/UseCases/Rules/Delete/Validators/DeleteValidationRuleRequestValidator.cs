using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Contracts;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Validators;

public sealed class DeleteValidationRuleRequestValidator : AbstractValidator<DeleteValidationRuleRequest>
{
    public DeleteValidationRuleRequestValidator()
    {
        RuleFor(request => request.RuleId).NotEmpty();
    }
}
