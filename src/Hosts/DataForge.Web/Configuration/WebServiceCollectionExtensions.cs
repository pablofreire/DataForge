using System.Threading.RateLimiting;
using DataForge.BuildingBlocks.Web.ErrorHandling;
using DataForge.BuildingBlocks.Web.Security;
using DataForge.BuildingBlocks.Web.Tenancy;
using DataForge.Modules.Identity.Presentation.Controllers;
using DataForge.Web.Composition;
using DataForge.Web.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace DataForge.Web.Configuration;

public static class WebServiceCollectionExtensions
{
    public static IServiceCollection AddDataForgeWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorComponents().AddInteractiveServerComponents();
        services.AddCascadingAuthenticationState();
        services.AddScoped<CircuitHandler, TenantCircuitHandler>();

        // ControllersWithViews registers the MVC antiforgery filters used to protect cookie-authenticated APIs from CSRF.
        var mvc = services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
        foreach (var assembly in PresentationAssemblies.All)
        {
            mvc.AddApplicationPart(assembly);
        }

        services.AddAntiforgery(options => options.HeaderName = AntiforgeryDefaults.HeaderName);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddDataForgeAuthentication();
        services.AddDataForgeRateLimiting(configuration);

        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            // Shared key ring so every web replica can read cookies issued by the others.
            services.AddDataProtection().SetApplicationName("DataForge").PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }

        return services;
    }

    private static void AddDataForgeAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/access-denied";
                options.Cookie.Name = "dataforge.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                // APIs answer with status codes; only browser pages are redirected.
                options.Events.OnRedirectToLogin = context => RespondOrRedirect(context.HttpContext, context.RedirectUri, StatusCodes.Status401Unauthorized);
                options.Events.OnRedirectToAccessDenied = context => RespondOrRedirect(context.HttpContext, context.RedirectUri, StatusCodes.Status403Forbidden);
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.TenantUser, policy => policy.RequireAuthenticatedUser().RequireClaim(DataForgeClaimTypes.TenantId))
            .AddPolicy(AuthorizationPolicies.ImplementationEngineer, policy => policy.RequireAuthenticatedUser().RequireRole(DataForgeRoles.ImplementationEngineer));
    }

    private static void AddDataForgeRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(AuthController.LoginRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.LoginPermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        });
    }

    private static Task RespondOrRedirect(HttpContext context, string redirectUri, int statusCode)
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        }

        context.Response.Redirect(redirectUri);
        return Task.CompletedTask;
    }
}
