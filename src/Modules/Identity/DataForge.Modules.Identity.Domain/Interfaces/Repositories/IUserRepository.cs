using DataForge.Modules.Identity.Domain.Entities;

namespace DataForge.Modules.Identity.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    /// <summary>Looks up an active user by normalized e-mail across all tenants (login has no tenant yet).</summary>
    Task<UserEntity?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task UpdateLastLoginAsync(long userId, DateTimeOffset loggedInAt, CancellationToken cancellationToken);

    /// <summary>Inserts the user unless the e-mail already exists. Returns true when inserted.</summary>
    Task<bool> AddIfNotExistsAsync(UserEntity user, CancellationToken cancellationToken);
}
