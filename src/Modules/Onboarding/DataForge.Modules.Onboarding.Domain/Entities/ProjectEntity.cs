using DataForge.BuildingBlocks.Domain.Entities;

namespace DataForge.Modules.Onboarding.Domain.Entities;

/// <summary>
/// An onboarding project: the container for the datasets a customer must load into the target system
/// and for the validation rules those datasets must satisfy.
/// </summary>
public sealed class ProjectEntity : TenantOwnedEntity
{
    public const int NameMaxLength = 120;
    public const int DescriptionMaxLength = 1000;

    private ProjectEntity()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public static ProjectEntity Create(long tenantId, string name, string? description, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var project = new ProjectEntity
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        };
        project.Initialize(now);
        return project;
    }
}
