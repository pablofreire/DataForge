using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Contracts;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.GetById.Interfaces;

public interface IGetJobByIdUseCase : IUseCase<GetJobByIdRequest, GetJobByIdResponse>;
