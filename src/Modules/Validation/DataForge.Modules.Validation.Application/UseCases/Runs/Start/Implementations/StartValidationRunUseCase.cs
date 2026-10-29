using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Validation.Application.Services;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Interfaces;
using DataForge.Modules.Validation.Domain.Errors;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.Start.Implementations;

/// <summary>Manually (re)validates an imported dataset against the project's current rules.</summary>
public sealed class StartValidationRunUseCase(
    IOnboardingModuleApi onboardingApi,
    IValidationRuleRepository ruleRepository,
    ValidationRunScheduler runScheduler,
    ITenantContext tenantContext,
    IValidator<StartValidationRunRequest> validator) : IStartValidationRunUseCase
{
    public async Task<Result<StartValidationRunResponse>> ExecuteAsync(StartValidationRunRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var dataset = await onboardingApi.GetDatasetAsync(request.DatasetId, cancellationToken);
        if (dataset is null)
        {
            return ValidationRunErrors.DatasetNotFound;
        }

        if (dataset.Status != "Imported")
        {
            return ValidationRunErrors.DatasetNotImported;
        }

        var rules = await ruleRepository.ListByProjectAsync(tenantContext.TenantId, dataset.ProjectId, cancellationToken);
        if (rules.Count == 0)
        {
            return ValidationRunErrors.NoRules;
        }

        var (run, job) = await runScheduler.ScheduleAsync(dataset.DatasetId, dataset.ProjectId, cancellationToken);
        return new StartValidationRunResponse(run.PublicId, job.JobId);
    }
}
