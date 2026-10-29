using System.Threading.Channels;

namespace DataForge.BuildingBlocks.Application.Pipelines;

/// <summary>
/// Runs a producer and a consumer concurrently, connected by a bounded channel.
/// When the consumer falls behind, the channel fills up and the producer awaits (backpressure),
/// so memory stays bounded by <c>capacity</c> regardless of the dataset size.
/// A failure on either side cancels the other and is rethrown to the caller.
/// </summary>
public static class BoundedChannelPipeline
{
    public static async Task<long> RunAsync<TItem>(
        IAsyncEnumerable<TItem> source,
        Func<TItem, CancellationToken, ValueTask> sink,
        int capacity,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        var channel = Channel.CreateBounded<TItem>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = linked.Token;

        var producer = ProduceAsync(source, channel.Writer, token);
        var consumer = ConsumeAsync(channel.Reader, sink, token);

        try
        {
            var completed = await Task.WhenAny(producer, consumer);
            if (completed.IsFaulted || completed.IsCanceled)
            {
                await linked.CancelAsync();
            }

            await Task.WhenAll(producer, consumer);
            return await consumer;
        }
        catch
        {
            await linked.CancelAsync();
            throw;
        }
    }

    private static async Task ProduceAsync<TItem>(
        IAsyncEnumerable<TItem> source,
        ChannelWriter<TItem> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                await writer.WriteAsync(item, cancellationToken);
            }

            writer.TryComplete();
        }
        catch (Exception exception)
        {
            writer.TryComplete(exception);
            throw;
        }
    }

    private static async Task<long> ConsumeAsync<TItem>(
        ChannelReader<TItem> reader,
        Func<TItem, CancellationToken, ValueTask> sink,
        CancellationToken cancellationToken)
    {
        long consumed = 0;
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
        {
            await sink(item, cancellationToken);
            consumed++;
        }

        return consumed;
    }
}
