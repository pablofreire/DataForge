using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DataForge.BuildingBlocks.Web.Middleware;

/// <summary>
/// Accepts an incoming <c>X-Correlation-Id</c> (or generates one), echoes it in the response and adds it
/// to the logging scope so every log line of the request can be correlated.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";
    private const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = context.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > MaxLength || !correlationId.All(IsSafe))
        {
            correlationId = context.TraceIdentifier;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }

    private static bool IsSafe(char character) => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or ':' or '.';
}
