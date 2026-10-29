using DataForge.Modules.Processing.Contracts.Models;

namespace DataForge.Modules.Processing.Contracts.Interfaces;

/// <summary>
/// Executes one type of job. Implementations live in the module that owns the data (for example the
/// Onboarding module imports datasets); the Processing module only provides the execution engine.
/// <para>
/// Executors must be idempotent and resumable: they receive the last committed checkpoint and must
/// produce the same final state whether they run once or are resumed after a crash.
/// </para>
/// </summary>
public interface IJobExecutor
{
    string JobType { get; }

    Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
}
