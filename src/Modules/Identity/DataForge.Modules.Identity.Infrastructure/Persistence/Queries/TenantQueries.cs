namespace DataForge.Modules.Identity.Infrastructure.Persistence.Queries;

internal static class TenantQueries
{
    private const string Columns = "id, public_id, name, slug, is_active, created_at, updated_at";

    public const string GetById = $"""
        SELECT {Columns} FROM identity.tenants WHERE id = @TenantId AND is_active = true;
        """;

    public const string GetByIds = $"""
        SELECT {Columns} FROM identity.tenants WHERE id = ANY(@TenantIds);
        """;

    public const string AddOrGetBySlug = $"""
        WITH inserted AS (
            INSERT INTO identity.tenants (public_id, name, slug, created_at, updated_at)
            VALUES (@PublicId, @Name, @Slug, @CreatedAt, @UpdatedAt)
            ON CONFLICT (slug) DO NOTHING
            RETURNING {Columns}
        )
        SELECT {Columns} FROM inserted
        UNION ALL
        SELECT {Columns} FROM identity.tenants WHERE slug = @Slug
        LIMIT 1;
        """;
}
