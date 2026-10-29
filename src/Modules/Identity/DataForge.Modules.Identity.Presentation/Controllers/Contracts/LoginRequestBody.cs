namespace DataForge.Modules.Identity.Presentation.Controllers.Contracts;

public sealed record LoginRequestBody(string Email, string Password)
{
    public override string ToString() => $"{nameof(LoginRequestBody)} {{ Email = {Email} }}";
}
