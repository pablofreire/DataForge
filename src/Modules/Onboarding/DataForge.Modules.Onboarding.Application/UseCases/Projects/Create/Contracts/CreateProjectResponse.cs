namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.Create.Contracts;

public sealed record CreateProjectResponse(Guid ProjectId, string Name, string? Description, DateTimeOffset CreatedAt);
