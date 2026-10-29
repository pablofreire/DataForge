using DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Issues.List.Validators;

public sealed class ListValidationIssuesRequestValidator : AbstractValidator<ListValidationIssuesRequest>
{
    public ListValidationIssuesRequestValidator()
    {
        RuleFor(request => request.DatasetId).NotEmpty();
        RuleFor(request => request.Limit).InclusiveBetween(1, 500);
        RuleFor(request => request.Cursor).MaximumLength(100);
        RuleFor(request => request.ColumnName).MaximumLength(200);
    }
}
