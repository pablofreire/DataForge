using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.ReadModels;

namespace DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;

public interface IDatasetRepository
{
    /// <summary>Inserts the dataset. Returns false when identical content already exists in the project (concurrent duplicate upload).</summary>
    Task<bool> AddAsync(DatasetEntity dataset, CancellationToken cancellationToken);

    Task<DatasetEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken);

    /// <summary>Finds an active dataset with identical content in the same project (idempotent re-upload).</summary>
    Task<DatasetEntity?> GetByContentHashAsync(long tenantId, long projectId, string contentSha256, CancellationToken cancellationToken);

    Task<IReadOnlyList<DatasetEntity>> ListByProjectAsync(long tenantId, long projectId, CancellationToken cancellationToken);

    /// <summary>Persists status, columns, row count and failure reason.</summary>
    Task UpdateImportStateAsync(DatasetEntity dataset, CancellationToken cancellationToken);

    Task<DatasetTotals> GetTotalsAsync(long tenantId, CancellationToken cancellationToken);
}
