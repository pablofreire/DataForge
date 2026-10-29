using DataForge.Modules.Identity.Application.Interfaces;
using DataForge.Modules.Identity.Domain.Entities;
using DataForge.Modules.Identity.Domain.Enums;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;
using DataForge.Modules.Identity.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataForge.Modules.Identity.Infrastructure.Seeding;

/// <summary>
/// Creates the demo tenants and accounts used by local development, the end-to-end tests and the README.
/// Idempotent: existing tenants and users are left untouched.
/// </summary>
public sealed partial class IdentityDemoDataSeeder(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IOptions<DemoSeedOptions> options,
    TimeProvider timeProvider,
    ILogger<IdentityDemoDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var password = options.Value.Password;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            throw new InvalidOperationException($"{DemoSeedOptions.SectionName}:Password must be configured with at least 12 characters.");
        }

        var now = timeProvider.GetUtcNow();
        var passwordHash = passwordHasher.Hash(password);

        var acme = await tenantRepository.AddOrGetBySlugAsync(TenantEntity.Create("Acme Corporation", "acme", now), cancellationToken);
        var globex = await tenantRepository.AddOrGetBySlugAsync(TenantEntity.Create("Globex Industries", "globex", now), cancellationToken);
        var internalTeam = await tenantRepository.AddOrGetBySlugAsync(TenantEntity.Create("DataForge Implementation", "dataforge-internal", now), cancellationToken);

        UserEntity[] users =
        [
            UserEntity.Create(acme.Id, "admin@acme.demo", "Alice Admin", passwordHash, UserRole.TenantAdmin, now),
            UserEntity.Create(acme.Id, "analyst@acme.demo", "Arthur Analyst", passwordHash, UserRole.TenantMember, now),
            UserEntity.Create(globex.Id, "admin@globex.demo", "Grace Admin", passwordHash, UserRole.TenantAdmin, now),
            UserEntity.Create(internalTeam.Id, "engineer@dataforge.demo", "Ian Engineer", passwordHash, UserRole.ImplementationEngineer, now),
        ];

        foreach (var user in users)
        {
            if (await userRepository.AddIfNotExistsAsync(user, cancellationToken))
            {
                LogUserCreated(logger, user.Email, user.Role);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo user {Email} created with role {Role}")]
    private static partial void LogUserCreated(ILogger logger, string email, UserRole role);
}
