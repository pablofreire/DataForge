namespace DataForge.Modules.Processing.Domain.Services;

/// <summary>
/// Exponential backoff for transient job failures: base, 2x base, 4x base ... capped at <c>maxDelay</c>.
/// </summary>
public static class JobRetryPolicy
{
    public static TimeSpan ComputeDelay(int attempt, TimeSpan baseDelay, TimeSpan maxDelay)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(attempt);

        // Cap the exponent to avoid overflow for pathological attempt counts.
        var exponent = Math.Min(attempt - 1, 16);
        var delay = TimeSpan.FromTicks(baseDelay.Ticks * (1L << exponent));
        return delay > maxDelay ? maxDelay : delay;
    }
}
