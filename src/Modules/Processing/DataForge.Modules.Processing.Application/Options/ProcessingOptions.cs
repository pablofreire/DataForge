using System.ComponentModel.DataAnnotations;

namespace DataForge.Modules.Processing.Application.Options;

public sealed class ProcessingOptions
{
    public const string SectionName = "Processing";

    /// <summary>Maximum number of jobs a single worker process runs concurrently.</summary>
    [Range(1, 64)]
    public int MaxConcurrentJobs { get; set; } = 2;

    /// <summary>Delay between polls when the queue is empty.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>How long a claimed job is owned before another worker may take it over.</summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Hard limit for a single attempt; exceeding it counts as a (retryable) failure.</summary>
    public TimeSpan JobTimeout { get; set; } = TimeSpan.FromHours(2);

    [Range(1, 20)]
    public int MaxAttempts { get; set; } = 3;

    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan HeartbeatInterval => LeaseDuration / 3;
}
