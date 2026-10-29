using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Interfaces;

public interface IGetProjectByIdUseCase : IUseCase<GetProjectByIdRequest, GetProjectByIdResponse>;
