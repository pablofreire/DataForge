namespace DataForge.Modules.Onboarding.Infrastructure.Persistence.Queries;

internal static class DatasetQueries
{
    private const string Columns = """
        d.id, d.public_id, d.tenant_id, d.project_id, p.public_id AS project_public_id, d.file_name,
        d.content_sha256, d.size_bytes, d.storage_path, d.status, d.row_count, d.columns, d.failure_reason,
        d.imported_at, d.is_active, d.created_at, d.updated_at
        """;

    public const string Insert = """
        INSERT INTO onboarding.datasets (
            public_id, tenant_id, project_id, file_name, content_sha256, size_bytes, storage_path, status,
            created_at, updated_at)
        VALUES (
            @PublicId, @TenantId, @ProjectId, @FileName, @ContentSha256, @SizeBytes, @StoragePath, @Status,
            @CreatedAt, @UpdatedAt)
        ON CONFLICT (project_id, content_sha256) WHERE is_active = true DO NOTHING
        RETURNING id;
        """;

    public const string GetByPublicId = $"""
        SELECT {Columns}
        FROM onboarding.datasets d
        JOIN onboarding.projects p ON p.id = d.project_id
        WHERE d.tenant_id = @TenantId AND d.public_id = @PublicId AND d.is_active = true;
        """;

    public const string GetByContentHash = $"""
        SELECT {Columns}
        FROM onboarding.datasets d
        JOIN onboarding.projects p ON p.id = d.project_id
        WHERE d.tenant_id = @TenantId AND d.project_id = @ProjectId AND d.content_sha256 = @ContentSha256 AND d.is_active = true;
        """;

    public const string ListByProject = $"""
        SELECT {Columns}
        FROM onboarding.datasets d
        JOIN onboarding.projects p ON p.id = d.project_id
        WHERE d.tenant_id = @TenantId AND d.project_id = @ProjectId AND d.is_active = true
        ORDER BY d.id DESC;
        """;

    public const string UpdateImportState = """
        UPDATE onboarding.datasets
        SET status = @Status,
            row_count = @RowCount,
            columns = @Columns,
            failure_reason = @FailureReason,
            imported_at = @ImportedAt,
            updated_at = @UpdatedAt
        WHERE id = @Id AND tenant_id = @TenantId;
        """;

    public const string Totals = """
        SELECT (SELECT count(*) FROM onboarding.projects WHERE tenant_id = @TenantId AND is_active = true)::int AS ProjectCount,
               count(*)::int AS DatasetCount,
               COALESCE(sum(row_count), 0)::bigint AS RowCount
        FROM onboarding.datasets
        WHERE tenant_id = @TenantId AND is_active = true;
        """;
}
