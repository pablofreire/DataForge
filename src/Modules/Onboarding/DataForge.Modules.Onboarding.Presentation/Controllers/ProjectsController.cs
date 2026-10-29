using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Upload.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Modules.Onboarding.Presentation.Controllers;

[Route("api/projects")]
[Authorize(Policy = AuthorizationPolicies.TenantUser)]
public sealed class ProjectsController(
    IListProjectsUseCase listProjects,
    ICreateProjectUseCase createProject,
    IGetProjectByIdUseCase getProjectById,
    IListProjectDatasetsUseCase listProjectDatasets,
    IUploadDatasetUseCase uploadDataset) : BaseApiController
{
    [HttpGet]
    public Task<IActionResult> List(CancellationToken cancellationToken) =>
        ExecuteAsync(() => listProjects.ExecuteAsync(new ListProjectsRequest(), cancellationToken));

    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => createProject.ExecuteAsync(request, cancellationToken), StatusCodes.Status201Created);

    [HttpGet("{projectId:guid}")]
    public Task<IActionResult> Get(Guid projectId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => getProjectById.ExecuteAsync(new GetProjectByIdRequest(projectId), cancellationToken));

    [HttpGet("{projectId:guid}/datasets")]
    public Task<IActionResult> ListDatasets(Guid projectId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => listProjectDatasets.ExecuteAsync(new ListProjectDatasetsRequest(projectId), cancellationToken));

    /// <summary>
    /// Uploads a dataset as the raw request body (<c>Content-Type: text/csv</c>), streamed straight to storage.
    /// The body size limit is enforced by the use case while streaming, so the server-wide limit is lifted here.
    /// </summary>
    [HttpPost("{projectId:guid}/datasets")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public Task<IActionResult> Upload(Guid projectId, [FromQuery] string fileName, CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => uploadDataset.ExecuteAsync(new UploadDatasetRequest(projectId, fileName, Request.Body), cancellationToken),
            StatusCodes.Status202Accepted);
}
