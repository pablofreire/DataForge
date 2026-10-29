using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Contracts;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListRecent.Interfaces;

public interface IListRecentJobsUseCase : IUseCase<ListRecentJobsRequest, ListRecentJobsResponse>;
