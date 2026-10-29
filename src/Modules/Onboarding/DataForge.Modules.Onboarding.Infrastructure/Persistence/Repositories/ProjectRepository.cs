using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Onboarding.Domain.Entities;
using DataForge.Modules.Onboarding.Domain.Interfaces.Repositories;
using DataForge.Modules.Onboarding.Domain.ReadModels;
using DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(IDbConnectionFactory connectionFactory) : IProjectRepository
{
    public async Task<bool> AddAsync(ProjectEntity project, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(project);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var id = await connection.ExecuteScalarAsync<long?>(new CommandDefinition(
            ProjectQueries.Insert,
            new { project.PublicId, project.TenantId, project.Name, project.Description, project.CreatedAt, project.UpdatedAt },
            cancellationToken: cancellationToken));
        return id.HasValue;
    }

    public async Task<ProjectEntity?> GetByPublicIdAsync(long tenantId, Guid publicId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProjectEntity>(new CommandDefinition(
            ProjectQueries.GetByPublicId,
            new { TenantId = tenantId, PublicId = publicId },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<ProjectListItem>> ListAsync(long tenantId, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var projects = await connection.QueryAsync<ProjectListItem>(new CommandDefinition(
            ProjectQueries.List,
            new { TenantId = tenantId },
            cancellationToken: cancellationToken));
        return projects.AsList();
    }
}
