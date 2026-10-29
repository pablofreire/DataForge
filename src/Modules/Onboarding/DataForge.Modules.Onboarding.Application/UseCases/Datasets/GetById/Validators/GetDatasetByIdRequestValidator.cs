using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Contracts;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Validators;

public sealed class GetDatasetByIdRequestValidator : AbstractValidator<GetDatasetByIdRequest>
{
    public GetDatasetByIdRequestValidator()
    {
        RuleFor(request => request.DatasetId).NotEmpty();
    }
}
