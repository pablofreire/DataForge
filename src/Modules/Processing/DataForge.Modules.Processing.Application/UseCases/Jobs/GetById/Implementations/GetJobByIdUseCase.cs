using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Processing.Application.Mappings;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Contracts;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Interfaces;
using DataForge.Modules.Processing.Domain.Errors;
using DataForge.Modules.Processing.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Implementations;

public sealed class GetJobByIdUseCase(
    IJobRepository jobRepository,
    ITenantContext tenantContext,
    IValidator<GetJobByIdRequest> validator) : IGetJobByIdUseCase
{
    public async Task<Result<GetJobByIdResponse>> ExecuteAsync(GetJobByIdRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var job = await jobRepository.GetByPublicIdAsync(tenantContext.TenantId, request.JobId, cancellationToken);
        return job is null
            ? JobErrors.NotFound
            : new GetJobByIdResponse(job.ToSummaryDto());
    }
}
