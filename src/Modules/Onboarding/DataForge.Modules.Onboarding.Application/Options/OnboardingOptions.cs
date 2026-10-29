using System.ComponentModel.DataAnnotations;

namespace DataForge.Modules.Onboarding.Application.Options;

public sealed class OnboardingOptions
{
    public const string SectionName = "Onboarding";

    /// <summary>Maximum accepted upload size in bytes (default 2 GiB).</summary>
    [Range(1, long.MaxValue)]
    public long MaxUploadBytes { get; set; } = 2L * 1024 * 1024 * 1024;

    /// <summary>Rows per COPY batch. Larger batches mean fewer round trips but more memory per batch.</summary>
    [Range(100, 100_000)]
    public int ImportBatchSize { get; set; } = 5_000;

    /// <summary>Batches buffered between the CSV reader and the database writer (backpressure bound).</summary>
    [Range(1, 64)]
    public int ImportChannelCapacity { get; set; } = 4;
}
