namespace DataForge.Modules.Validation.Infrastructure.Persistence.Queries;

internal static class ValidationRuleQueries
{
    public const string Insert = """
        INSERT INTO validation.rules (public_id, tenant_id, project_public_id, column_name, rule_type, parameters, created_at, updated_at)
        VALUES (@PublicId, @TenantId, @ProjectPublicId, @ColumnName, @RuleType, @Parameters, @CreatedAt, @UpdatedAt);
        """;

    public const string ListByProject = """
        SELECT id, public_id, tenant_id, project_public_id, column_name, rule_type, parameters, is_active, created_at, updated_at
        FROM validation.rules
        WHERE tenant_id = @TenantId AND project_public_id = @ProjectPublicId AND is_active = true
        ORDER BY id;
        """;

    public const string Deactivate = """
        UPDATE validation.rules
        SET is_active = false, updated_at = @Now
        WHERE tenant_id = @TenantId AND public_id = @PublicId AND is_active = true;
        """;
}
