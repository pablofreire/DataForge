namespace DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;

/// <summary>
/// The authenticated principal. <see cref="TenantId"/> is the internal tenant key; the presentation layer
/// stores it only inside the encrypted authentication cookie and never returns it from an API.
/// </summary>
public sealed record AuthenticateUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    long TenantId,
    Guid TenantPublicId,
    string TenantName);
