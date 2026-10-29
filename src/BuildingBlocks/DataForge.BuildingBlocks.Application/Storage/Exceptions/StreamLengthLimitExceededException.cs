namespace DataForge.BuildingBlocks.Application.Storage.Exceptions;

public sealed class StreamLengthLimitExceededException : Exception
{
    public StreamLengthLimitExceededException()
    {
    }

    public StreamLengthLimitExceededException(string message)
        : base(message)
    {
    }

    public StreamLengthLimitExceededException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public StreamLengthLimitExceededException(long maxLength)
        : base($"The stream exceeded the maximum length of {maxLength} bytes.")
    {
        MaxLength = maxLength;
    }

    public long MaxLength { get; }
}
