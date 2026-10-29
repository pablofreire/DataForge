using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.Start.Contracts;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.Start.Interfaces;

public interface IStartValidationRunUseCase : IUseCase<StartValidationRunRequest, StartValidationRunResponse>;
