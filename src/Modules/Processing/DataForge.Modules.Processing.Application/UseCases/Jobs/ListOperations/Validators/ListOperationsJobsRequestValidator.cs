using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Contracts;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Validators;

public sealed class ListOperationsJobsRequestValidator : AbstractValidator<ListOperationsJobsRequest>
{
    public ListOperationsJobsRequestValidator()
    {
        RuleFor(request => request.Limit).InclusiveBetween(1, 500);
    }
}
