using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Interfaces;
using DataForge.Modules.Processing.Domain.Enums;
using DataForge.Modules.Processing.Domain.Errors;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Implementations;

/// <summary>
/// Cancels a queued job immediately; a running job is flagged and its worker stops at the next heartbeat,
/// keeping the last checkpoint so the work can be resumed later.
/// </summary>
public sealed class CancelJobUseCase(
    IJobRepository jobRepository,
    ITenantContext tenantContext,
    IValidator<CancelJobRequest> validator) : ICancelJobUseCase
{
    public async Task<Result<CancelJobResponse>> ExecuteAsync(CancelJobRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var job = await jobRepository.GetByPublicIdAsync(tenantContext.TenantId, request.JobId, cancellationToken);
        if (job is null)
        {
            return JobErrors.NotFound;
        }

        if (!job.CanBeCancelled || !await jobRepository.RequestCancelAsync(tenantContext.TenantId, request.JobId, cancellationToken))
        {
            return JobErrors.NotCancellable;
        }

        var updated = await jobRepository.GetByPublicIdAsync(tenantContext.TenantId, request.JobId, cancellationToken);
        return new CancelJobResponse(request.JobId, (updated?.Status ?? JobStatus.Cancelled).ToString());
    }
}
