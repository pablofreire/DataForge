using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Interfaces;
using DataForge.Modules.Validation.Domain.Errors;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Implementations;

/// <summary>Soft-deletes a rule. Completed runs keep their issues, so history stays intact.</summary>
public sealed class DeleteValidationRuleUseCase(
    IValidationRuleRepository ruleRepository,
    ITenantContext tenantContext,
    IValidator<DeleteValidationRuleRequest> validator,
    TimeProvider timeProvider) : IDeleteValidationRuleUseCase
{
    public async Task<Result<DeleteValidationRuleResponse>> ExecuteAsync(DeleteValidationRuleRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var deleted = await ruleRepository.DeactivateAsync(tenantContext.TenantId, request.RuleId, timeProvider.GetUtcNow(), cancellationToken);
        return deleted ? new DeleteValidationRuleResponse(request.RuleId) : ValidationRuleErrors.NotFound;
    }
}
