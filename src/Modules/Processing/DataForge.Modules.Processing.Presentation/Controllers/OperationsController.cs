using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Modules.Processing.Presentation.Controllers;

/// <summary>Implementation console endpoints: cross-tenant visibility for the internal team only.</summary>
[Route("api/operations")]
[Authorize(Policy = AuthorizationPolicies.ImplementationEngineer)]
public sealed class OperationsController(IListOperationsJobsUseCase listOperationsJobs) : BaseApiController
{
    [HttpGet("jobs")]
    public Task<IActionResult> ListJobs([FromQuery] int limit = 100, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => listOperationsJobs.ExecuteAsync(new ListOperationsJobsRequest(limit), cancellationToken));
}
