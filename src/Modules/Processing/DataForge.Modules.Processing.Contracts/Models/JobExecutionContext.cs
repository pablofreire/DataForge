using DataForge.Modules.Processing.Contracts.Interfaces;

namespace DataForge.Modules.Processing.Contracts.Models;

/// <param name="JobId">Public identifier of the job.</param>
/// <param name="ResourceId">Public identifier of the resource the job works on.</param>
/// <param name="Attempt">1-based attempt number.</param>
/// <param name="CheckpointPosition">Last position committed by a previous attempt (0 when starting fresh).</param>
/// <param name="Progress">Persists progress and checkpoints for this job.</param>
public sealed record JobExecutionContext(
    Guid JobId,
    Guid ResourceId,
    int Attempt,
    long CheckpointPosition,
    IJobProgressReporter Progress);
