using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.ReadModels;

namespace DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;

public interface IProjectRepository
{
    /// <summary>Inserts the project. Returns false when the tenant already has a project with the same name.</summary>
    Task<bool> AddAsync(ProjectEntity project, CancellationToken cancellationToken);

    Task<ProjectEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProjectListItem>> ListAsync(long tenantId, CancellationToken cancellationToken);
}
