namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;

internal static class ProjectQueries
{
    public const string Insert = """
        INSERT INTO onboarding.projects (public_id, tenant_id, name, description, created_at, updated_at)
        VALUES (@PublicId, @TenantId, @Name, @Description, @CreatedAt, @UpdatedAt)
        ON CONFLICT (tenant_id, lower(name)) WHERE is_active = true DO NOTHING
        RETURNING id;
        """;

    public const string GetByPublicId = """
        SELECT id, public_id, tenant_id, name, description, is_active, created_at, updated_at
        FROM onboarding.projects
        WHERE tenant_id = @TenantId AND public_id = @PublicId AND is_active = true;
        """;

    public const string List = """
        SELECT p.public_id AS PublicId,
               p.name AS Name,
               p.description AS Description,
               COALESCE(d.dataset_count, 0)::int AS DatasetCount,
               COALESCE(d.total_rows, 0)::bigint AS TotalRows,
               p.created_at AS CreatedAt
        FROM onboarding.projects p
        LEFT JOIN LATERAL (
            SELECT count(*) AS dataset_count, sum(row_count) AS total_rows
            FROM onboarding.datasets
            WHERE project_id = p.id AND is_active = true
        ) d ON true
        WHERE p.tenant_id = @TenantId AND p.is_active = true
        ORDER BY p.created_at DESC;
        """;
}
