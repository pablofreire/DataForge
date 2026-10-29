using DataForge.BuildingBlocks.Application.Tenancy.Interfaces;
using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Validation.Application.Mappings;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Issues.Shared;
using DataForge.Modules.Validation.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Validation.Application.UseCases.Issues.List.Implementations;

/// <summary>Keyset-paginated issues of the latest run of a dataset, optionally filtered by column.</summary>
public sealed class ListValidationIssuesUseCase(
    IValidationRunRepository runRepository,
    IValidationIssueRepository issueRepository,
    ITenantContext tenantContext,
    IValidator<ListValidationIssuesRequest> validator) : IListValidationIssuesUseCase
{
    private static readonly Error InvalidCursor = Error.Validation("issues.invalid_cursor", "The pagination cursor is invalid.");

    public async Task<Result<ListValidationIssuesResponse>> ExecuteAsync(ListValidationIssuesRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return validationError;
        }

        if (!IssueCursor.TryDecode(request.Cursor, out var cursor))
        {
            return InvalidCursor;
        }

        var run = await runRepository.GetLatestForDatasetAsync(tenantContext.TenantId, request.DatasetId, cancellationToken);
        if (run is null)
        {
            return new ListValidationIssuesResponse(null, [], null);
        }

        var issues = await issueRepository.ListAsync(
            run.Id,
            cursor.RowNumber,
            cursor.RuleId,
            request.Limit,
            string.IsNullOrWhiteSpace(request.ColumnName) ? null : request.ColumnName.Trim(),
            cancellationToken);

        var nextCursor = issues.Count == request.Limit
            ? new IssueCursor(issues[^1].RowNumber, issues[^1].RuleId).Encode()
            : null;

        return new ListValidationIssuesResponse(run.PublicId, issues.Select(issue => issue.ToItem()).ToList(), nextCursor);
    }
}
