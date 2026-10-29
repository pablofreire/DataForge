using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Contracts;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Interfaces;

public interface IListOperationsJobsUseCase : IUseCase<ListOperationsJobsRequest, ListOperationsJobsResponse>;
