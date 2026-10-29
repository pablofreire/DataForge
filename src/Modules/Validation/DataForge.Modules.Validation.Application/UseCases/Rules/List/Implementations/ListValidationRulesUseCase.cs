using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Validation.Application.Mappings;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Interfaces;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.List.Implementations;

public sealed class ListValidationRulesUseCase(
    IValidationRuleRepository ruleRepository,
    IOnboardingModuleApi onboardingApi,
    ITenantContext tenantContext,
    IValidator<ListValidationRulesRequest> validator) : IListValidationRulesUseCase
{
    private static readonly Error ProjectNotFound = Error.NotFound("project.not_found", "The project was not found.");

    public async Task<Result<ListValidationRulesResponse>> ExecuteAsync(ListValidationRulesRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        if (await onboardingApi.GetProjectAsync(request.ProjectId, cancellationToken) is null)
        {
            return ProjectNotFound;
        }

        var rules = await ruleRepository.ListByProjectAsync(tenantContext.TenantId, request.ProjectId, cancellationToken);
        return new ListValidationRulesResponse(rules.Select(rule => rule.ToItem()).ToList());
    }
}
