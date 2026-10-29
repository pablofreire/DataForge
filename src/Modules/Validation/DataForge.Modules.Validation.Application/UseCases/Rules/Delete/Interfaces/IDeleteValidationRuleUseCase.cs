using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Contracts;

namespace DataForge.Modules.Validation.Application.UseCases.Rules.Delete.Interfaces;

public interface IDeleteValidationRuleUseCase : IUseCase<DeleteValidationRuleRequest, DeleteValidationRuleResponse>;
