namespace DataForge.Modules.Processing.Domain.ValueObjects;

/// <summary>
/// Result of a lease renewal. <see cref="IsOwned"/> is false when another worker took over the job
/// (for example after a long GC pause), in which case the current worker must stop immediately.
/// </summary>
public readonly record struct JobLeaseState(bool IsOwned, bool CancelRequested);
