namespace DataForge.Modules.Processing.Application.Exceptions;

/// <summary>
/// Raised when a worker tries to write progress for a job it no longer owns.
/// </summary>
public sealed class JobLeaseLostException : Exception
{
    public JobLeaseLostException()
    {
    }

    public JobLeaseLostException(string message)
        : base(message)
    {
    }

    public JobLeaseLostException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public JobLeaseLostException(long jobId)
        : base($"The lease of job {jobId} was lost to another worker.")
    {
    }
}
