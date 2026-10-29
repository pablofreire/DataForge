using DataForge.BuildingBlocks.Application.UseCases.Interfaces;
using DataForge.Modules.Validation.Application.UseCases.Issues.List.Contracts;

namespace DataForge.Modules.Validation.Application.UseCases.Issues.List.Interfaces;

public interface IListValidationIssuesUseCase : IUseCase<ListValidationIssuesRequest, ListValidationIssuesResponse>;
