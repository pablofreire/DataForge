using System.Security.Cryptography;
using DataForge.BuildingBlocks.Application.Storage.Exceptions;

namespace DataForge.BuildingBlocks.Application.Storage.Streams;

/// <summary>
/// Read-only pass-through stream that computes a SHA-256 hash and enforces a maximum length while the
/// content is being copied. This allows a single pass over an upload: no buffering, no second read.
/// </summary>
public sealed class HashingReadStream(Stream inner, long maxLength) : Stream
{
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private long _bytesRead;

    public long BytesRead => _bytesRead;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => _bytesRead;
        set => throw new NotSupportedException();
    }

    /// <summary>Returns the lowercase hex SHA-256 of everything read so far.</summary>
    public string GetHashHex() => Convert.ToHexStringLower(_hash.GetCurrentHash());

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        var read = inner.Read(buffer);
        Track(buffer[..read]);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await inner.ReadAsync(buffer, cancellationToken);
        Track(buffer.Span[..read]);
        return read;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hash.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Track(ReadOnlySpan<byte> data)
    {
        _bytesRead += data.Length;
        if (_bytesRead > maxLength)
        {
            throw new StreamLengthLimitExceededException(maxLength);
        }

        _hash.AppendData(data);
    }
}
