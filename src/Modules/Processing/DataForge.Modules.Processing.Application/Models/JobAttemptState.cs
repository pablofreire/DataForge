namespace DataForge.Modules.Processing.Application.Models;

/// <summary>
/// Signals raised by the heartbeat loop and read by the runner when the attempt is cancelled.
/// </summary>
internal sealed class JobAttemptState
{
    private volatile bool _leaseLost;
    private volatile bool _cancelRequested;

    public bool LeaseLost => _leaseLost;

    public bool CancelRequested => _cancelRequested;

    public void MarkLeaseLost() => _leaseLost = true;

    public void MarkCancelRequested() => _cancelRequested = true;
}
