using System.ComponentModel.DataAnnotations;

namespace DataForge.Modules.Validation.Application.Options;

public sealed class ValidationOptions
{
    public const string SectionName = "Validation";

    /// <summary>Rows read per keyset query.</summary>
    [Range(100, 100_000)]
    public int BatchSize { get; set; } = 5_000;

    /// <summary>Batches buffered between the database reader and the validators (backpressure bound).</summary>
    [Range(1, 64)]
    public int ChannelCapacity { get; set; } = 4;

    /// <summary>Degree of parallelism used to validate the rows of one batch. 0 means one per CPU core.</summary>
    [Range(0, 256)]
    public int DegreeOfParallelism { get; set; }

    /// <summary>Maximum time a customer-supplied regular expression may spend on one value.</summary>
    public TimeSpan RegexTimeout { get; set; } = TimeSpan.FromMilliseconds(50);

    public int EffectiveDegreeOfParallelism => DegreeOfParallelism > 0 ? DegreeOfParallelism : Environment.ProcessorCount;
}
