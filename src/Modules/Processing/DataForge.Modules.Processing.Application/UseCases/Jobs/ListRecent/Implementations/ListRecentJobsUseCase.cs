using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Processing.Application.Mappings;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Interfaces;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Implementations;

public sealed class ListRecentJobsUseCase(
    IJobRepository jobRepository,
    ITenantContext tenantContext,
    IValidator<ListRecentJobsRequest> validator) : IListRecentJobsUseCase
{
    public async Task<Result<ListRecentJobsResponse>> ExecuteAsync(ListRecentJobsRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var jobs = await jobRepository.ListRecentAsync(tenantContext.TenantId, request.Limit, cancellationToken);
        return new ListRecentJobsResponse(jobs.Select(job => job.ToSummaryDto()).ToList());
    }
}
