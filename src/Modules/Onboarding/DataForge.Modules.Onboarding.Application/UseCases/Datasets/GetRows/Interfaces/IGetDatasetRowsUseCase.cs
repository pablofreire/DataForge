using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetRows.Interfaces;

public interface IGetDatasetRowsUseCase : IUseCase<GetDatasetRowsRequest, GetDatasetRowsResponse>;
