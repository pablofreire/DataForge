using DataForge.Modules.Onboarding.Application.UseCases.Projects.Shared;

namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.List.Contracts;

public sealed record ListProjectsResponse(IReadOnlyList<ProjectItem> Projects);
