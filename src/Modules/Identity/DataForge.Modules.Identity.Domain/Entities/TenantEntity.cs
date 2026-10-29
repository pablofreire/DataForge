using DataForge.BuildingBlocks.Domain.Entities;

namespace DataForge.Modules.Identity.Domain.Entities;

/// <summary>
/// A customer organization (or the internal implementation team). All customer data is partitioned by tenant.
/// </summary>
public sealed class TenantEntity : Entity
{
    private TenantEntity()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public static TenantEntity Create(string name, string slug, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var tenant = new TenantEntity
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
        };
        tenant.Initialize(now);
        return tenant;
    }
}
