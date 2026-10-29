using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Contracts;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Validators;

public sealed class ListRecentJobsRequestValidator : AbstractValidator<ListRecentJobsRequest>
{
    public ListRecentJobsRequestValidator()
    {
        RuleFor(request => request.Limit).InclusiveBetween(1, 200);
    }
}
