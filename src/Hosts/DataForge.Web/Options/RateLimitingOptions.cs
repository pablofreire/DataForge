namespace DataForge.Web.Options;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Login attempts allowed per client IP per minute (brute-force protection).</summary>
    public int LoginPermitsPerMinute { get; set; } = 10;
}
