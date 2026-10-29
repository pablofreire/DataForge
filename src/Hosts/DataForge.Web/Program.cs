using DataForge.BuildingBlocks.Infrastructure.DependencyInjection;
using DataForge.BuildingBlocks.Infrastructure.Observability;
using DataForge.BuildingBlocks.Web.Middleware;
using DataForge.BuildingBlocks.Web.Tenancy;
using DataForge.Modules.Identity.Application.DependencyInjection;
using DataForge.Modules.Identity.Infrastructure.DependencyInjection;
using DataForge.Modules.Onboarding.Application.DependencyInjection;
using DataForge.Modules.Onboarding.Infrastructure.DependencyInjection;
using DataForge.Modules.Processing.Application.DependencyInjection;
using DataForge.Modules.Processing.Infrastructure.DependencyInjection;
using DataForge.Modules.Validation.Application.DependencyInjection;
using DataForge.Modules.Validation.Infrastructure.DependencyInjection;
using DataForge.Web.Components;
using DataForge.Web.Composition;
using DataForge.Web.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

// DataForge.Web: Blazor customer portal and implementation console, plus the REST API (MVC controllers).
// Background processing runs in DataForge.Worker; this host only enqueues jobs.
var builder = WebApplication.CreateBuilder(args);

builder.AddDataForgeObservability(
    "dataforge-web",
    tracing => tracing
        .AddAspNetCoreInstrumentation(options => options.Filter = context => !context.Request.Path.StartsWithSegments("/health", StringComparison.Ordinal))
        .AddHttpClientInstrumentation(),
    metrics => metrics.AddAspNetCoreInstrumentation());

builder.Services
    .AddBuildingBlocksInfrastructure(builder.Configuration)
    .AddIdentityApplication()
    .AddIdentityInfrastructure()
    .AddOnboardingApplication()
    .AddOnboardingInfrastructure()
    .AddValidationApplication()
    .AddValidationInfrastructure()
    .AddProcessingApplication()
    .AddProcessingInfrastructure()
    .AddDataForgeWeb(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler("/error", createScopeForErrors: true);
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options => options.GetLevel = (context, _, exception) =>
    exception is not null || context.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error
    : context.Request.Path.StartsWithSegments("/health", StringComparison.Ordinal) ? Serilog.Events.LogEventLevel.Verbose
    : Serilog.Events.LogEventLevel.Information);

app.MapStaticAssets();

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies([.. PresentationAssemblies.All]);

await app.RunAsync();

