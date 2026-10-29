using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.Mappings;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Interfaces;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Implementations;

public sealed class ListProjectDatasetsUseCase(
    IProjectRepository projectRepository,
    IDatasetRepository datasetRepository,
    ITenantContext tenantContext,
    IValidator<ListProjectDatasetsRequest> validator) : IListProjectDatasetsUseCase
{
    public async Task<Result<ListProjectDatasetsResponse>> ExecuteAsync(ListProjectDatasetsRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var project = await projectRepository.GetByPublicIdAsync(tenantContext.TenantId, request.ProjectId, cancellationToken);
        if (project is null)
        {
            return ProjectErrors.NotFound;
        }

        var datasets = await datasetRepository.ListByProjectAsync(tenantContext.TenantId, project.Id, cancellationToken);
        return new ListProjectDatasetsResponse(datasets.Select(dataset => dataset.ToItem()).ToList());
    }
}
