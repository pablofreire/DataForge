using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Identity.Contracts.Interfaces;
using DataForge.Modules.Processing.Application.Mappings;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Interfaces;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Implementations;

/// <summary>
/// Cross-tenant job list for the implementation console. Authorization (implementation engineers only)
/// is enforced by the presentation layer policy.
/// </summary>
public sealed class ListOperationsJobsUseCase(
    IJobRepository jobRepository,
    ITenantDirectory tenantDirectory,
    IValidator<ListOperationsJobsRequest> validator) : IListOperationsJobsUseCase
{
    public async Task<Result<ListOperationsJobsResponse>> ExecuteAsync(ListOperationsJobsRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var jobs = await jobRepository.ListRecentAsync(tenantId: null, request.Limit, cancellationToken);
        var tenants = await tenantDirectory.GetByIdsAsync(jobs.Select(job => job.TenantId).Distinct().ToArray(), cancellationToken);

        var items = jobs
            .Select(job =>
            {
                var tenant = tenants.GetValueOrDefault(job.TenantId);
                return new OperationsJobItem(tenant?.TenantId ?? job.TenantPublicId, tenant?.Name ?? "Unknown tenant", job.ToSummaryDto());
            })
            .ToList();

        return new ListOperationsJobsResponse(items);
    }
}
