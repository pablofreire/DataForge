using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Datasets.GetById.Interfaces;

public interface IGetDatasetByIdUseCase : IUseCase<GetDatasetByIdRequest, GetDatasetByIdResponse>;
