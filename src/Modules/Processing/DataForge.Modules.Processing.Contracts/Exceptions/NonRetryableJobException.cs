namespace DataForge.Modules.Processing.Contracts.Exceptions;

/// <summary>
/// Thrown by an executor when retrying cannot succeed (for example, a malformed file).
/// The job is marked as failed immediately instead of being retried.
/// </summary>
public sealed class NonRetryableJobException : Exception
{
    public NonRetryableJobException()
    {
    }

    public NonRetryableJobException(string message)
        : base(message)
    {
    }

    public NonRetryableJobException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
