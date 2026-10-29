using DataForge.Modules.Onboarding.Contracts.Dtos;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Contracts;

public sealed record GetDatasetRowsResponse(IReadOnlyList<string> Columns, IReadOnlyList<DatasetRowDto> Rows, long? NextCursor);
