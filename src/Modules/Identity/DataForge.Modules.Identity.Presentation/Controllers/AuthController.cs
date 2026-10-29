using System.Security.Claims;
using DataForge.BuildingBlocks.Domain.Results;
using DataForge.BuildingBlocks.Web.Api;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Interfaces;
using DataForge.Modules.Identity.Presentation.Authentication;
using DataForge.Modules.Identity.Presentation.Controllers.Contracts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DataForge.Modules.Identity.Presentation.Controllers;

/// <summary>
/// Cookie-based authentication for API clients (scripts, load tests) and the browser logout form.
/// Unsafe requests require an antiforgery token, obtained from <c>GET /api/auth/antiforgery</c>.
/// </summary>
public sealed class AuthController(
    IAuthenticateUserUseCase authenticateUser,
    IAntiforgery antiforgery) : BaseApiController
{
    public const string LoginRateLimitPolicy = "login";

    [HttpGet("/api/auth/antiforgery")]
    [AllowAnonymous]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(ApiResponse.Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName }, HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/auth/login")]
    [AllowAnonymous]
    [EnableRateLimiting(LoginRateLimitPolicy)]
    public Task<IActionResult> Login([FromBody] LoginRequestBody body, CancellationToken cancellationToken) =>
        ExecuteAsync(async () =>
        {
            var result = await authenticateUser.ExecuteAsync(new AuthenticateUserRequest(body.Email, body.Password), cancellationToken);
            if (result.IsFailure)
            {
                return Result.Failure<CurrentUserResponse>(result.Error);
            }

            await AuthenticationCookie.SignInAsync(HttpContext, result.Value);
            var user = result.Value;
            return Result.Success(new CurrentUserResponse(user.UserId, user.Email, user.DisplayName, user.Role, user.TenantPublicId, user.TenantName));
        });

    [HttpPost("/api/auth/logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await AuthenticationCookie.SignOutAsync(HttpContext);
        return NoContent();
    }

    /// <summary>Logout from the browser layout (HTML form post), then back to the login page.</summary>
    [HttpPost("/account/logout")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> LogoutForm()
    {
        await AuthenticationCookie.SignOutAsync(HttpContext);
        return LocalRedirect("/account/login");
    }

    [HttpGet("/api/auth/me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var user = new CurrentUserResponse(
            Guid.Parse(User.FindFirstValue(DataForgeClaimTypes.UserPublicId)!),
            User.FindFirstValue(ClaimTypes.Email)!,
            User.FindFirstValue(ClaimTypes.Name)!,
            User.FindFirstValue(ClaimTypes.Role)!,
            Guid.Parse(User.FindFirstValue(DataForgeClaimTypes.TenantPublicId)!),
            User.FindFirstValue(DataForgeClaimTypes.TenantName)!);
        return Ok(ApiResponse.Ok(user, HttpContext.TraceIdentifier));
    }
}
