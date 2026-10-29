using DataForge.BuildingBlocks.Domain.Results;

namespace DataForge.Modules.Processing.Domain.Errors;

public static class JobErrors
{
    public static readonly Error NotFound = Error.NotFound("job.not_found", "The job was not found.");

    public static readonly Error NotCancellable = Error.Conflict("job.not_cancellable", "Only queued or running jobs can be cancelled.");
}
