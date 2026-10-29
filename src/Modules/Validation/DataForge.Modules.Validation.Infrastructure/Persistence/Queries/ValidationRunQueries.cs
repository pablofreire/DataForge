namespace DataForge.Modules.Validation.Infrastructure.Persistence.Queries;

internal static class ValidationRunQueries
{
    private const string Columns = """
        id, public_id, tenant_id, dataset_public_id, project_public_id, status, total_rows, validated_rows,
        invalid_rows, issue_count, rule_count, failure_reason, started_at, completed_at, is_active, created_at, updated_at
        """;

    public const string Insert = """
        INSERT INTO validation.runs (public_id, tenant_id, dataset_public_id, project_public_id, status, created_at, updated_at)
        VALUES (@PublicId, @TenantId, @DatasetPublicId, @ProjectPublicId, @Status, @CreatedAt, @UpdatedAt);
        """;

    public const string GetByPublicId = $"""
        SELECT {Columns} FROM validation.runs
        WHERE tenant_id = @TenantId AND public_id = @PublicId AND is_active = true;
        """;

    public const string GetLatestForDataset = $"""
        SELECT {Columns} FROM validation.runs
        WHERE tenant_id = @TenantId AND dataset_public_id = @DatasetPublicId AND is_active = true
        ORDER BY id DESC
        LIMIT 1;
        """;

    public const string Update = """
        UPDATE validation.runs
        SET status = @Status, total_rows = @TotalRows, validated_rows = @ValidatedRows, invalid_rows = @InvalidRows,
            issue_count = @IssueCount, rule_count = @RuleCount, failure_reason = @FailureReason,
            started_at = @StartedAt, completed_at = @CompletedAt, updated_at = @UpdatedAt
        WHERE id = @Id AND tenant_id = @TenantId;
        """;

    /// <summary>Recomputes the per-rule breakdown of a run (idempotent).</summary>
    public const string RefreshRuleStats = """
        DELETE FROM validation.run_rule_stats WHERE run_id = @RunId;

        INSERT INTO validation.run_rule_stats (run_id, rule_id, column_name, rule_type, issue_count)
        SELECT run_id, rule_id, min(column_name), min(rule_type), count(*)
        FROM validation.issues
        WHERE run_id = @RunId
        GROUP BY run_id, rule_id;
        """;

    public const string Totals = """
        SELECT count(DISTINCT row_number) FILTER (WHERE row_number > 0) AS InvalidRows,
               count(*) AS IssueCount
        FROM validation.issues
        WHERE run_id = @RunId;
        """;

    public const string ListRuleStats = """
        SELECT r.public_id AS RuleId, s.column_name AS ColumnName, s.rule_type AS RuleType, s.issue_count AS IssueCount
        FROM validation.run_rule_stats s
        JOIN validation.rules r ON r.id = s.rule_id
        WHERE s.run_id = @RunId
        ORDER BY s.issue_count DESC, s.column_name;
        """;
}
