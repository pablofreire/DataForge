using Dapper;
using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using DataForge.Modules.Identity.Domain.Entities;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;
using DataForge.Modules.Identity.Infrastructure.Persistence.Queries;

namespace DataForge.Modules.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<UserEntity?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserEntity>(new CommandDefinition(
            UserQueries.GetByEmail,
            new { Email = normalizedEmail },
            cancellationToken: cancellationToken));
    }

    public async Task UpdateLastLoginAsync(long userId, DateTimeOffset loggedInAt, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            UserQueries.UpdateLastLogin,
            new { UserId = userId, LoggedInAt = loggedInAt },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> AddIfNotExistsAsync(UserEntity user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(
            UserQueries.InsertIfNotExists,
            new
            {
                user.PublicId,
                user.TenantId,
                user.Email,
                user.DisplayName,
                user.PasswordHash,
                Role = user.Role.ToString(),
                user.CreatedAt,
                user.UpdatedAt,
            },
            cancellationToken: cancellationToken));
        return affected == 1;
    }
}
