using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Contracts;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Interfaces;
using DataForge.Modules.Onboarding.Contracts.Interfaces;
using DataForge.Modules.Onboarding.Domain.Errors;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Implementations;

/// <summary>Keyset-paginated preview of staged rows.</summary>
public sealed class GetDatasetRowsUseCase(
    IDatasetRepository datasetRepository,
    IDatasetRowReader rowReader,
    ITenantContext tenantContext,
    IValidator<GetDatasetRowsRequest> validator) : IGetDatasetRowsUseCase
{
    public async Task<Result<GetDatasetRowsResponse>> ExecuteAsync(GetDatasetRowsRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        var dataset = await datasetRepository.GetByPublicIdAsync(tenantContext.TenantId, request.DatasetId, cancellationToken);
        if (dataset is null)
        {
            return DatasetErrors.NotFound;
        }

        var rows = await rowReader.ReadBatchAsync(dataset.PublicId, request.AfterRowNumber, request.Limit, cancellationToken);
        long? nextCursor = rows.Count == request.Limit ? rows[^1].RowNumber : null;
        return new GetDatasetRowsResponse(dataset.Columns, rows, nextCursor);
    }
}
