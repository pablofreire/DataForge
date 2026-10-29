namespace DataForge.Modules.Validation.Application.UseCases.Runs.Start.Contracts;

public sealed record StartValidationRunResponse(Guid RunId, Guid JobId);
