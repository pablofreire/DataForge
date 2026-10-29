using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Contracts;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Validators;

public sealed class GetDatasetRowsRequestValidator : AbstractValidator<GetDatasetRowsRequest>
{
    public GetDatasetRowsRequestValidator()
    {
        RuleFor(request => request.DatasetId).NotEmpty();
        RuleFor(request => request.AfterRowNumber).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Limit).InclusiveBetween(1, 500);
    }
}
