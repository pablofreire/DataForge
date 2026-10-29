namespace DataForge.Modules.Onboarding.Domain.ReadModels;

/// <summary>Read model for the project list (project plus dataset aggregates).</summary>
public sealed record ProjectListItem(
    Guid PublicId,
    string Name,
    string? Description,
    int DatasetCount,
    long TotalRows,
    DateTimeOffset CreatedAt);
