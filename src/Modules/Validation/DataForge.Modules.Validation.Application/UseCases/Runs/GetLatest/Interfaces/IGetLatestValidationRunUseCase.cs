using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Contracts;

namespace DataForge.Modules.Validation.Application.UseCases.Runs.GetLatest.Interfaces;

public interface IGetLatestValidationRunUseCase : IUseCase<GetLatestValidationRunRequest, GetLatestValidationRunResponse>;
