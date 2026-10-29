using DataForge.Modules.Validation.Domain.Engine.Models;

namespace DataForge.Modules.Validation.Domain.Interfaces.Repositories;

public interface IValidationIssueRepository
{
    /// <summary>
    /// Keyset page of issues ordered by (row number, rule). The cursor is the (row number, rule id) of the last
    /// issue of the previous page.
    /// </summary>
    Task<IReadOnlyList<ValidationIssue>> ListAsync(
        long runId,
        long afterRowNumber,
        long afterRuleId,
        int limit,
        string? columnName,
        CancellationToken cancellationToken);
}
