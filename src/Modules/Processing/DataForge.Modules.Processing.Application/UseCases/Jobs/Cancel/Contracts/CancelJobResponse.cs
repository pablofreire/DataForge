namespace DataForge.Modules.Processing.Application.UseCases.Jobs.Cancel.Contracts;

public sealed record CancelJobResponse(Guid JobId, string Status);
