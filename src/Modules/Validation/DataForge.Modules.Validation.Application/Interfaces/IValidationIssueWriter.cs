using DataForge.Modules.Validation.Domain.Engine.Models;

namespace DataForge.Modules.Validation.Application.Interfaces;

/// <summary>Bulk writer for validation issues (PostgreSQL binary COPY in production).</summary>
public interface IValidationIssueWriter
{
    Task WriteAsync(long runId, IReadOnlyCollection<ValidationIssue> issues, CancellationToken cancellationToken);

    /// <summary>Deletes every issue of the run (fresh start).</summary>
    Task DeleteAllAsync(long runId, CancellationToken cancellationToken);

    /// <summary>Deletes row-level issues after <paramref name="rowNumber"/> (resume from a checkpoint).</summary>
    Task DeleteRowIssuesAfterAsync(long runId, long rowNumber, CancellationToken cancellationToken);

    /// <summary>Deletes the issues produced by one rule (re-running a set-based rule).</summary>
    Task DeleteRuleIssuesAsync(long runId, long ruleId, CancellationToken cancellationToken);
}
