namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Contracts;

public sealed record GetDatasetRowsRequest(Guid DatasetId, long AfterRowNumber, int Limit);
