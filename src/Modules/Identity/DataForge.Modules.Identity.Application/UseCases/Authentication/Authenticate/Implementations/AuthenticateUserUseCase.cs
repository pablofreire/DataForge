using DataForge.BuildingBlocks.Application.Validation;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.Modules.Identity.Application.Interfaces;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Interfaces;
using DataForge.Modules.Identity.Domain.Entities;
using DataForge.Modules.Identity.Domain.Errors;
using DataForge.Modules.Identity.Domain.Interfaces.Repositories;
using FluentValidation;

namespace DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Implementations;

/// <summary>
/// Verifies credentials. Unknown e-mails still pay the cost of a hash verification so response time
/// does not reveal which accounts exist (user enumeration through timing).
/// </summary>
public sealed class AuthenticateUserUseCase(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IPasswordHasher passwordHasher,
    IValidator<AuthenticateUserRequest> validator,
    TimeProvider timeProvider) : IAuthenticateUserUseCase
{
    // Hash of a random value, computed once and used to equalize timing for unknown accounts.
    private static string? _dummyHash;

    public async Task<Result<AuthenticateUserResponse>> ExecuteAsync(AuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        var validationError = await validator.ValidateToErrorAsync(request, cancellationToken);
        if (validationError != Error.None)
        {
            return UserErrors.InvalidCredentials;
        }

        var user = await userRepository.GetByEmailAsync(UserEntity.NormalizeEmail(request.Email), cancellationToken);
        if (user is null)
        {
            _dummyHash ??= passwordHasher.Hash(Guid.NewGuid().ToString("N"));
            passwordHasher.Verify(request.Password, _dummyHash);
            return UserErrors.InvalidCredentials;
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return UserErrors.InvalidCredentials;
        }

        var tenant = await tenantRepository.GetByIdAsync(user.TenantId, cancellationToken);
        if (tenant is null)
        {
            return UserErrors.InvalidCredentials;
        }

        var now = timeProvider.GetUtcNow();
        user.RecordLogin(now);
        await userRepository.UpdateLastLoginAsync(user.Id, now, cancellationToken);

        return new AuthenticateUserResponse(
            user.PublicId,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            tenant.Id,
            tenant.PublicId,
            tenant.Name);
    }
}
