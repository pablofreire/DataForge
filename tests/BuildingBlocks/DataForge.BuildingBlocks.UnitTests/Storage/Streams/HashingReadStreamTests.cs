using System.Security.Cryptography;
using System.Text;
using DataForge.BuildingBlocks.Application.Storage.Exceptions;
using DataForge.BuildingBlocks.Application.Storage.Streams;

namespace DataForge.BuildingBlocks.UnitTests.Storage.Streams;

public sealed class HashingReadStreamTests
{
    [Fact]
    public async Task CopyToAsync_ComputesSha256AndLengthOfContent()
    {
        var content = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("id,name\n1,Alice\n", 5_000)));
        await using var hashing = new HashingReadStream(new MemoryStream(content), maxLength: content.Length);
        await using var target = new MemoryStream();

        await hashing.CopyToAsync(target, TestContext.Current.CancellationToken);

        hashing.BytesRead.Should().Be(content.Length);
        hashing.GetHashHex().Should().Be(Convert.ToHexStringLower(SHA256.HashData(content)));
        target.ToArray().Should().Equal(content);
    }

    [Fact]
    public async Task CopyToAsync_ContentLongerThanLimit_Throws()
    {
        await using var hashing = new HashingReadStream(new MemoryStream(new byte[1_000]), maxLength: 999);

        var act = () => hashing.CopyToAsync(Stream.Null, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<StreamLengthLimitExceededException>();
    }
}
