using DataForge.BuildingBlocks.Domain.Entities;
using DataForge.Modules.Identity.Domain.Enums;

namespace DataForge.Modules.Identity.Domain.Entities;

public sealed class UserEntity : TenantOwnedEntity
{
    private UserEntity()
    {
    }

    /// <summary>Normalized (trimmed, lower-case) e-mail; unique across all tenants.</summary>
    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public static UserEntity Create(long tenantId, string email, string displayName, string passwordHash, UserRole role, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new UserEntity
        {
            TenantId = tenantId,
            Email = NormalizeEmail(email),
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
        };
        user.Initialize(now);
        return user;
    }

    public static string NormalizeEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Trim().ToLowerInvariant();
    }

    public void RecordLogin(DateTimeOffset now)
    {
        LastLoginAt = now;
        Touch(now);
    }
}
