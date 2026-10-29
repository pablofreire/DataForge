using System.Diagnostics.Metrics;

namespace DataForge.Modules.Processing.Application.Diagnostics;

/// <summary>
/// Business and technical metrics of the processing engine, exported through OpenTelemetry.
/// </summary>
public sealed class ProcessingMetrics
{
    public const string MeterName = "DataForge.Processing";

    private readonly Counter<long> _jobsStarted;
    private readonly Counter<long> _jobsFinished;
    private readonly Counter<long> _itemsProcessed;
    private readonly Histogram<double> _jobDuration;

    public ProcessingMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create(MeterName);
        _jobsStarted = meter.CreateCounter<long>("dataforge.jobs.started", unit: "{job}", description: "Job attempts started.");
        _jobsFinished = meter.CreateCounter<long>("dataforge.jobs.finished", unit: "{job}", description: "Job attempts finished, by outcome.");
        _itemsProcessed = meter.CreateCounter<long>("dataforge.jobs.items_processed", unit: "{item}", description: "Items (rows) processed by jobs.");
        _jobDuration = meter.CreateHistogram<double>("dataforge.jobs.duration", unit: "s", description: "Duration of job attempts.");
    }

    public void JobStarted(string jobType) => _jobsStarted.Add(1, new KeyValuePair<string, object?>("job.type", jobType));

    public void JobFinished(string jobType, string outcome, TimeSpan duration)
    {
        var tags = new[] { new KeyValuePair<string, object?>("job.type", jobType), new KeyValuePair<string, object?>("job.outcome", outcome) };
        _jobsFinished.Add(1, tags);
        _jobDuration.Record(duration.TotalSeconds, tags);
    }

    public void ItemsProcessed(string jobType, long count)
    {
        if (count > 0)
        {
            _itemsProcessed.Add(count, new KeyValuePair<string, object?>("job.type", jobType));
        }
    }
}
