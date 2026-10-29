using System.Runtime.CompilerServices;
using DataForge.BuildingBlocks.Application.Pipelines;

namespace DataForge.BuildingBlocks.UnitTests.Pipelines;

public sealed class BoundedChannelPipelineTests
{
    [Fact]
    public async Task RunAsync_ConsumesEveryItemInOrder()
    {
        var consumed = new List<int>();

        var count = await BoundedChannelPipeline.RunAsync(
            Produce(100),
            (item, _) =>
            {
                consumed.Add(item);
                return ValueTask.CompletedTask;
            },
            capacity: 4,
            TestContext.Current.CancellationToken);

        count.Should().Be(100);
        consumed.Should().Equal(Enumerable.Range(1, 100));
    }

    [Fact]
    public async Task RunAsync_SlowConsumer_ProducerIsThrottledByCapacity()
    {
        const int capacity = 3;
        var produced = 0;
        var consumed = 0;
        var maxInFlight = 0;

        await BoundedChannelPipeline.RunAsync(
            ProduceTracked(50, () => Interlocked.Increment(ref produced)),
            async (_, token) =>
            {
                // Items in flight = produced but not yet consumed: channel buffer + item being written + item being consumed.
                maxInFlight = Math.Max(maxInFlight, Volatile.Read(ref produced) - consumed);
                await Task.Delay(2, token);
                consumed++;
            },
            capacity,
            TestContext.Current.CancellationToken);

        consumed.Should().Be(50);
        maxInFlight.Should().BeLessThanOrEqualTo(capacity + 2, "backpressure must bound memory regardless of dataset size");
    }

    [Fact]
    public async Task RunAsync_ConsumerFails_ExceptionPropagatesAndProducerStops()
    {
        var produced = 0;

        var act = () => BoundedChannelPipeline.RunAsync(
            ProduceTracked(10_000, () => Interlocked.Increment(ref produced)),
            (item, _) => item == 5 ? throw new InvalidOperationException("sink failed") : ValueTask.CompletedTask,
            capacity: 2,
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("sink failed");
        produced.Should().BeLessThan(100, "the producer must be cancelled when the consumer fails");
    }

    [Fact]
    public async Task RunAsync_ProducerFails_ExceptionPropagates()
    {
        var act = () => BoundedChannelPipeline.RunAsync(
            FailingProducer(),
            (_, _) => ValueTask.CompletedTask,
            capacity: 2,
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    private static async IAsyncEnumerable<int> Produce(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var item = 1; item <= count; item++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<int> ProduceTracked(int count, Action onProduced, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var item = 1; item <= count; item++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            onProduced();
            yield return item;
        }
    }

    private static async IAsyncEnumerable<int> FailingProducer()
    {
        yield return 1;
        await Task.Yield();
        throw new InvalidDataException("malformed input");
    }
}
