using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Interfaces;

public interface ICreateProjectUseCase : IUseCase<CreateProjectRequest, CreateProjectResponse>;
