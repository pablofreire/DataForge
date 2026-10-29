using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Modules.Validation.Presentation.Controllers;

[Route("api/datasets/{datasetId:guid}")]
[Authorize(Policy = AuthorizationPolicies.TenantUser)]
public sealed class ValidationRunsController(
    IStartValidationRunUseCase startRun,
    IGetLatestValidationRunUseCase getLatestRun,
    IListValidationIssuesUseCase listIssues) : BaseApiController
{
    [HttpPost("validation-runs")]
    public Task<IActionResult> Start(Guid datasetId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => startRun.ExecuteAsync(new StartValidationRunRequest(datasetId), cancellationToken), StatusCodes.Status202Accepted);

    [HttpGet("validation-runs/latest")]
    public Task<IActionResult> GetLatest(Guid datasetId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => getLatestRun.ExecuteAsync(new GetLatestValidationRunRequest(datasetId), cancellationToken));

    /// <summary>Keyset-paginated issues of the latest run. Pass the <c>nextCursor</c> of the previous page.</summary>
    [HttpGet("issues")]
    public Task<IActionResult> ListIssues(
        Guid datasetId,
        [FromQuery] string? cursor,
        [FromQuery] string? column,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => listIssues.ExecuteAsync(new ListValidationIssuesRequest(datasetId, cursor, limit, column), cancellationToken));
}
