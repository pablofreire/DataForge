using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Contracts;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.Create.Validators;

/// <summary>Shape validation only; rule-specific parameter rules live in <see cref="ValidationRuleEntity"/>.</summary>
public sealed class CreateValidationRuleRequestValidator : AbstractValidator<CreateValidationRuleRequest>
{
    public CreateValidationRuleRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
        RuleFor(request => request.ColumnName).NotEmpty().MaximumLength(ValidationRuleEntity.ColumnNameMaxLength);
        RuleFor(request => request.RuleType)
            .NotEmpty()
            .Must(value => Enum.TryParse<RuleType>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
            .WithMessage($"RuleType must be one of: {string.Join(", ", Enum.GetNames<RuleType>())}.");
    }
}
