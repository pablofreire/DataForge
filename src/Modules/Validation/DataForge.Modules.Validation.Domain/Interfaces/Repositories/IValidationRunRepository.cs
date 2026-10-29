using DataForge.Modules.Validation.Domain.Entities;
using DataForge.Modules.Validation.Domain.ReadModels;

namespace DataForge.Modules.Validation.Domain.Interfaces.Repositories;

public interface IValidationRunRepository
{
    Task AddAsync(ValidationRunEntity run, CancellationToken cancellationToken);

    Task<ValidationRunEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken);

    Task<ValidationRunEntity?> GetLatestForDatasetAsync(long tenantId, Guid datasetPublicId, CancellationToken cancellationToken);

    Task UpdateAsync(ValidationRunEntity run, CancellationToken cancellationToken);

    /// <summary>
    /// Computes the run totals (distinct invalid rows, issue count) and the per-rule breakdown in the database,
    /// storing the breakdown so result pages never aggregate millions of issues on read.
    /// </summary>
    Task<RunTotals> ComputeTotalsAsync(long runId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RuleIssueStat>> ListRuleStatsAsync(long runId, CancellationToken cancellationToken);
}
