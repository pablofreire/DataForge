using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Interfaces;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Implementations;

public sealed class CreateProjectUseCase(
    IProjectRepository projectRepository,
    ITenantContext tenantContext,
    IValidator<CreateProjectRequest> validator,
    TimeProvider timeProvider) : ICreateProjectUseCase
{
    public async Task<Result<CreateProjectResponse>> ExecuteAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var project = ProjectEntity.Create(tenantContext.TenantId, request.Name, request.Description, timeProvider.GetUtcNow());
        if (!await projectRepository.AddAsync(project, cancellationToken))
        {
            return ProjectErrors.NameAlreadyExists;
        }

        return new CreateProjectResponse(project.PublicId, project.Name, project.Description, project.CreatedAt);
    }
}
