using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.List.Contracts;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.List.Interfaces;

public interface IListValidationRulesUseCase : IUseCase<ListValidationRulesRequest, ListValidationRulesResponse>;
