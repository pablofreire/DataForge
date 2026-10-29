namespace DataForge.Modules.Identity.Infrastructure.Persistence.Queries;

internal static class UserQueries
{
    public const string GetByEmail = """
        SELECT id, public_id, tenant_id, email, display_name, password_hash, role, last_login_at,
               is_active, created_at, updated_at
        FROM identity.users
        WHERE email = @Email AND is_active = true;
        """;

    public const string UpdateLastLogin = """
        UPDATE identity.users
        SET last_login_at = @LoggedInAt, updated_at = @LoggedInAt
        WHERE id = @UserId;
        """;

    public const string InsertIfNotExists = """
        INSERT INTO identity.users (public_id, tenant_id, email, display_name, password_hash, role, created_at, updated_at)
        VALUES (@PublicId, @TenantId, @Email, @DisplayName, @PasswordHash, @Role, @CreatedAt, @UpdatedAt)
        ON CONFLICT (email) DO NOTHING;
        """;
}
