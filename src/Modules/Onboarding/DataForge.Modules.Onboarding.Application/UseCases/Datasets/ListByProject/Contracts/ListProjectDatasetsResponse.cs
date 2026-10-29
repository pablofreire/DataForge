using DataForge.Modules.Onboarding.Application.UseCases.Datasets.Shared;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Contracts;

public sealed record ListProjectDatasetsResponse(IReadOnlyList<DatasetItem> Datasets);
