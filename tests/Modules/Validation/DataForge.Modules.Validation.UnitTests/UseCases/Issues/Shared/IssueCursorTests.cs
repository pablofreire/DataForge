using DataForge.Modules.Validation.Application.UseCases.Issues.Shared;

namespace DataForge.Modules.Validation.UnitTests.UseCases.Issues.Shared;

public sealed class IssueCursorTests
{
    [Fact]
    public void EncodeThenDecode_RoundTrips()
    {
        var cursor = new IssueCursor(12_345, 67);

        IssueCursor.TryDecode(cursor.Encode(), out var decoded).Should().BeTrue();
        decoded.Should().Be(cursor);
    }

    [Fact]
    public void TryDecode_NoCursor_StartsBeforeDatasetLevelIssues()
    {
        IssueCursor.TryDecode(null, out var decoded).Should().BeTrue();
        decoded.RowNumber.Should().BeNegative("row 0 holds dataset-level issues and must be included on the first page");
    }

    [Theory]
    [InlineData("not-base64!")]
    [InlineData("Zm9v")]
    public void TryDecode_TamperedCursor_IsRejected(string token) =>
        IssueCursor.TryDecode(token, out _).Should().BeFalse();
}
