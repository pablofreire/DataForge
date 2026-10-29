using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Interfaces;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Implementations;

public sealed class GetProjectByIdUseCase(
    IProjectRepository projectRepository,
    ITenantContext tenantContext,
    IValidator<GetProjectByIdRequest> validator) : IGetProjectByIdUseCase
{
    public async Task<Result<GetProjectByIdResponse>> ExecuteAsync(GetProjectByIdRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var project = await projectRepository.GetByPublicIdAsync(tenantContext.TenantId, request.ProjectId, cancellationToken);
        return project is null
            ? ProjectErrors.NotFound
            : new GetProjectByIdResponse(project.PublicId, project.Name, project.Description, project.CreatedAt);
    }
}
