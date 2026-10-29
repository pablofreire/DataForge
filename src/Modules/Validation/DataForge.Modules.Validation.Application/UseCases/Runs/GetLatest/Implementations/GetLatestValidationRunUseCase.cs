using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Processing.Contracts.Interfaces;
using DataForge.Modules.Validation.Application.Jobs;
using DataForge.Modules.Validation.Application.Mappings;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Interfaces;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.Errors;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Implementations;

public sealed class GetLatestValidationRunUseCase(
    IOnboardingModuleApi onboardingApi,
    IValidationRunRepository runRepository,
    IJobQueryService jobQueryService,
    ITenantContext tenantContext,
    IValidator<GetLatestValidationRunRequest> validator) : IGetLatestValidationRunUseCase
{
    public async Task<Result<GetLatestValidationRunResponse>> ExecuteAsync(GetLatestValidationRunRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        if (await onboardingApi.GetDatasetAsync(request.DatasetId, cancellationToken) is null)
        {
            return ValidationRunErrors.DatasetNotFound;
        }

        var run = await runRepository.GetLatestForDatasetAsync(tenantContext.TenantId, request.DatasetId, cancellationToken);
        if (run is null)
        {
            return new GetLatestValidationRunResponse(null, [], null);
        }

        var stats = run.Status == ValidationRunStatus.Completed
            ? await runRepository.ListRuleStatsAsync(run.Id, cancellationToken)
            : [];
        var job = await jobQueryService.GetLatestForResourceAsync(run.PublicId, ValidationJobTypes.ValidateDataset, cancellationToken);

        return new GetLatestValidationRunResponse(run.ToItem(), stats.Select(stat => stat.ToItem()).ToList(), job);
    }
}
