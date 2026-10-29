using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.Create.Contracts;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.Create.Interfaces;

public interface ICreateValidationRuleUseCase : IUseCase<CreateValidationRuleRequest, CreateValidationRuleResponse>;
