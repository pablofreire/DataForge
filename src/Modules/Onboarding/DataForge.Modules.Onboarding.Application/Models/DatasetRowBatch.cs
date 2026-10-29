namespace DataForge.Modules.Onboarding.Application.Models;

/// <summary>A contiguous block of rows starting at <see cref="FirstRowNumber"/> (1-based).</summary>
public sealed record DatasetRowBatch(long FirstRowNumber, IReadOnlyList<string[]> Rows)
{
    public long LastRowNumber => FirstRowNumber + Rows.Count - 1;
}
