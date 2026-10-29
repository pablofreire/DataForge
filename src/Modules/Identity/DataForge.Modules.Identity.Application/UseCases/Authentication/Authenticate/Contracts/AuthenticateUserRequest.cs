namespace DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;

public sealed record AuthenticateUserRequest(string Email, string Password)
{
    // Keep the password out of logs and exception messages.
    public override string ToString() => $"{nameof(AuthenticateUserRequest)} {{ Email = {Email} }}";
}
