namespace DataForge.Modules.Onboarding.Contracts.Dtos;

/// <param name="RowNumber">1-based data row number (the header is not counted).</param>
/// <param name="Values">Raw field values in header order; may be shorter than the header for ragged rows.</param>
public sealed record DatasetRowDto(long RowNumber, IReadOnlyList<string?> Values);
