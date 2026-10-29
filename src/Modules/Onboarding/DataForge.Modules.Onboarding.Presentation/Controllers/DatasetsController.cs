using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Modules.Onboarding.Presentation.Controllers;

[Route("api/datasets")]
[Authorize(Policy = AuthorizationPolicies.TenantUser)]
public sealed class DatasetsController(
    IGetDatasetByIdUseCase getDatasetById,
    IGetDatasetRowsUseCase getDatasetRows) : BaseApiController
{
    [HttpGet("{datasetId:guid}")]
    public Task<IActionResult> Get(Guid datasetId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => getDatasetById.ExecuteAsync(new GetDatasetByIdRequest(datasetId), cancellationToken));

    /// <summary>Keyset-paginated rows: pass the <c>nextCursor</c> of the previous page as <paramref name="after"/>.</summary>
    [HttpGet("{datasetId:guid}/rows")]
    public Task<IActionResult> GetRows(Guid datasetId, [FromQuery] long after = 0, [FromQuery] int limit = 100, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => getDatasetRows.ExecuteAsync(new GetDatasetRowsRequest(datasetId, after, limit), cancellationToken));
}
