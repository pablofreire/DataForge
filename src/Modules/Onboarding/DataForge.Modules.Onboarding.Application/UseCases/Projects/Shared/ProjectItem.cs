namespace DataForge.Modules.Onboarding.Application.UseCases.Projects.Shared;

public sealed record ProjectItem(
    Guid ProjectId,
    string Name,
    string? Description,
    int DatasetCount,
    long TotalRows,
    DateTimeOffset CreatedAt);
