using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.Jobs;
using DataForge.Modules.Onboarding.Application.Mappings;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Interfaces;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Processing.Contracts.Interfaces;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Implementations;

public sealed class GetDatasetByIdUseCase(
    IDatasetRepository datasetRepository,
    IJobQueryService jobQueryService,
    ITenantContext tenantContext,
    IValidator<GetDatasetByIdRequest> validator) : IGetDatasetByIdUseCase
{
    public async Task<Result<GetDatasetByIdResponse>> ExecuteAsync(GetDatasetByIdRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var dataset = await datasetRepository.GetByPublicIdAsync(tenantContext.TenantId, request.DatasetId, cancellationToken);
        if (dataset is null)
        {
            return DatasetErrors.NotFound;
        }

        var importJob = await jobQueryService.GetLatestForResourceAsync(dataset.PublicId, OnboardingJobTypes.ImportDataset, cancellationToken);
        return new GetDatasetByIdResponse(dataset.ToItem(), importJob);
    }
}
