namespace DataForge.E2ETests.Fixtures;

/// <summary>Target environment. Defaults match deploy/compose/docker-compose.yml.</summary>
internal static class E2ESettings
{
    public static string BaseUrl => Environment.GetEnvironmentVariable("DATAFORGE_BASE_URL") ?? "http://localhost:8080";

    public static string DemoPassword => Environment.GetEnvironmentVariable("DATAFORGE_DEMO_PASSWORD") ?? "DataForge!Demo2026";

    /// <summary>Screenshots and traces are written here and published as CI artifacts.</summary>
    public static string EvidenceDirectory
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("DATAFORGE_EVIDENCE_DIR");
            var directory = string.IsNullOrWhiteSpace(configured)
                ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../docs/evidence/e2e"))
                : configured;
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
