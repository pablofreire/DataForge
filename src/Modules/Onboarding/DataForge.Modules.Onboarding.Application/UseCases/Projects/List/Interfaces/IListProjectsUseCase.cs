using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Contracts;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Interfaces;

public interface IListProjectsUseCase : IUseCase<ListProjectsRequest, ListProjectsResponse>;
