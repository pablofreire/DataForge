using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Modules.Processing.Presentation.Controllers;

[Route("api/jobs")]
[Authorize(Policy = AuthorizationPolicies.TenantUser)]
public sealed class JobsController(
    IListRecentJobsUseCase listRecentJobs,
    IGetJobByIdUseCase getJobById,
    ICancelJobUseCase cancelJob) : BaseApiController
{
    [HttpGet]
    public Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => listRecentJobs.ExecuteAsync(new ListRecentJobsRequest(limit), cancellationToken));

    [HttpGet("{jobId:guid}")]
    public Task<IActionResult> Get(Guid jobId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => getJobById.ExecuteAsync(new GetJobByIdRequest(jobId), cancellationToken));

    [HttpPost("{jobId:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid jobId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => cancelJob.ExecuteAsync(new CancelJobRequest(jobId), cancellationToken));
}
