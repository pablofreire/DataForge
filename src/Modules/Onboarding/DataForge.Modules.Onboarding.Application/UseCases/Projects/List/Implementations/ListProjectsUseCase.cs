using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.Mappings;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Interfaces;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Implementations;

public sealed class ListProjectsUseCase(IProjectRepository projectRepository, ITenantContext tenantContext) : IListProjectsUseCase
{
    public async Task<Result<ListProjectsResponse>> ExecuteAsync(ListProjectsRequest request, CancellationToken cancellationToken)
    {
        var projects = await projectRepository.ListAsync(tenantContext.TenantId, cancellationToken);
        return new ListProjectsResponse(projects.Select(project => project.ToItem()).ToList());
    }
}
