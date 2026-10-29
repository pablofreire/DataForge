namespace DataForge.BuildingBlocks.Domain.Entities;

/// <summary>
/// An aggregate that belongs to exactly one tenant. Every repository query filters by <see cref="TenantId"/>.
/// </summary>
public abstract class TenantOwnedEntity : Entity
{
    public long TenantId { get; protected set; }
}
