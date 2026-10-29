using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Validation.Application.Mappings;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Interfaces;
using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.Enums;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using DataForge.Modules.Validation.Domain.ValueObjects;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.Create.Implementations;

public sealed class CreateValidationRuleUseCase(
    IValidationRuleRepository ruleRepository,
    IOnboardingModuleApi onboardingApi,
    ITenantContext tenantContext,
    IValidator<CreateValidationRuleRequest> validator,
    TimeProvider timeProvider) : ICreateValidationRuleUseCase
{
    private static readonly Error ProjectNotFound = Error.NotFound("project.not_found", "The project was not found.");

    public async Task<Result<CreateValidationRuleResponse>> ExecuteAsync(CreateValidationRuleRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var project = await onboardingApi.GetProjectAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return ProjectNotFound;
        }

        var parameters = new RuleParameters
        {
            MaxLength = request.MaxLength,
            Pattern = request.Pattern,
            Min = request.Min,
            Max = request.Max,
            Format = request.Format,
            AllowedValues = request.AllowedValues,
        };

        var created = ValidationRuleEntity.Create(
            tenantContext.TenantId,
            project.ProjectId,
            request.ColumnName,
            Enum.Parse<RuleType>(request.RuleType, ignoreCase: true),
            parameters,
            timeProvider.GetUtcNow());

        if (created.IsFailure)
        {
            return created.Error;
        }

        await ruleRepository.AddAsync(created.Value, cancellationToken);
        return new CreateValidationRuleResponse(created.Value.ToItem());
    }
}
