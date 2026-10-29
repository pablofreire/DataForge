using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.ListByProject.Interfaces;

public interface IListProjectDatasetsUseCase : IUseCase<ListProjectDatasetsRequest, ListProjectDatasetsResponse>;
