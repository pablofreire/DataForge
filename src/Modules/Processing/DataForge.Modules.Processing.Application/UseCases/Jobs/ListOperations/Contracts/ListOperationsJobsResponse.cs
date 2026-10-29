namespace DataForge.Modules.Processing.Application.UseCases.Jobs.ListOperations.Contracts;

public sealed record ListOperationsJobsResponse(IReadOnlyList<OperationsJobItem> Jobs);
