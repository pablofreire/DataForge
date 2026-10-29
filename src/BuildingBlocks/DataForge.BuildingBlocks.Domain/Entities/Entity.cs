namespace DataForge.BuildingBlocks.Domain.Entities;

/// <summary>
/// Base type for aggregate roots that follow the dual-identifier convention:
/// <see cref="Id"/> is the internal surrogate key and is never exposed outside the process;
/// <see cref="PublicId"/> is the stable identifier used in routes, APIs and cross-module contracts.
/// </summary>
public abstract class Entity
{
    public long Id { get; protected set; }

    public Guid PublicId { get; protected set; }

    public bool IsActive { get; protected set; } = true;

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    protected void Initialize(DateTimeOffset now)
    {
        PublicId = Guid.CreateVersion7(now);
        IsActive = true;
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch(DateTimeOffset now) => UpdatedAt = now;
}
