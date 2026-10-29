using DataForge.Modules.Processing.Domain.Services;

namespace DataForge.Modules.Processing.UnitTests.Domain.Services;

public sealed class JobRetryPolicyTests
{
    private static readonly TimeSpan Base = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan Max = TimeSpan.FromMinutes(5);

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 20)]
    [InlineData(4, 40)]
    public void ComputeDelay_DoublesPerAttempt(int attempt, int expectedSeconds) =>
        JobRetryPolicy.ComputeDelay(attempt, Base, Max).Should().Be(TimeSpan.FromSeconds(expectedSeconds));

    [Theory]
    [InlineData(10)]
    [InlineData(1_000)]
    public void ComputeDelay_IsCappedAndNeverOverflows(int attempt) =>
        JobRetryPolicy.ComputeDelay(attempt, Base, Max).Should().Be(Max);
}
