namespace DataForge.Modules.Identity.Infrastructure.Options;

public sealed class DemoSeedOptions
{
    public const string SectionName = "DemoSeed";

    /// <summary>Password assigned to every demo account. Must come from configuration (environment variable or user secrets).</summary>
    public string Password { get; set; } = string.Empty;
}
