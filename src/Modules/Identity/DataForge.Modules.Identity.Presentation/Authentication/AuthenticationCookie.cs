using System.Globalization;
using System.Security.Claims;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.Modules.Identity.Application.UseCases.Authentication.Authenticate.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace DataForge.Modules.Identity.Presentation.Authentication;

/// <summary>
/// Issues and clears the authentication cookie. Shared by the login page and the auth API.
/// </summary>
public static class AuthenticationCookie
{
    public static async Task SignInAsync(HttpContext httpContext, AuthenticateUserResponse user)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(user);

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(DataForgeClaimTypes.UserPublicId, user.UserId.ToString()),
            new(DataForgeClaimTypes.TenantId, user.TenantId.ToString(CultureInfo.InvariantCulture)),
            new(DataForgeClaimTypes.TenantPublicId, user.TenantPublicId.ToString()),
            new(DataForgeClaimTypes.TenantName, user.TenantName),
        ];

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });
    }

    public static Task SignOutAsync(HttpContext httpContext) =>
        httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
