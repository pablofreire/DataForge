namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.GetById.Contracts;

public sealed record GetProjectByIdResponse(Guid ProjectId, string Name, string? Description, DateTimeOffset CreatedAt);
