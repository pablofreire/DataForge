using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Modules.Validation.Presentation.Controllers;

[Authorize(Policy = AuthorizationPolicies.TenantUser)]
public sealed class ValidationRulesController(
    IListValidationRulesUseCase listRules,
    ICreateValidationRuleUseCase createRule,
    IDeleteValidationRuleUseCase deleteRule) : BaseApiController
{
    [HttpGet("/api/projects/{projectId:guid}/rules")]
    public Task<IActionResult> List(Guid projectId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => listRules.ExecuteAsync(new ListValidationRulesRequest(projectId), cancellationToken));

    [HttpPost("/api/projects/{projectId:guid}/rules")]
    public Task<IActionResult> Create(Guid projectId, [FromBody] CreateValidationRuleRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => createRule.ExecuteAsync(request with { ProjectId = projectId }, cancellationToken), StatusCodes.Status201Created);

    [HttpDelete("/api/rules/{ruleId:guid}")]
    public Task<IActionResult> Delete(Guid ruleId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => deleteRule.ExecuteAsync(new DeleteValidationRuleRequest(ruleId), cancellationToken));
}
