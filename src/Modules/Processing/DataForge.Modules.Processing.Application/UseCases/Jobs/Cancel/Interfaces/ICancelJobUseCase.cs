using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Contracts;

namespace DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Interfaces;

public interface ICancelJobUseCase : IUseCase<CancelJobRequest, CancelJobResponse>;
