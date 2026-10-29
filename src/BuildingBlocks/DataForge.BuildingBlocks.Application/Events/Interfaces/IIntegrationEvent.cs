namespace DataForge.BuildingBlocks.Application.Events.Interfaces;

/// <summary>
/// A fact published by one module and consumed by others. Integration events are the only way modules react
/// to each other's state changes; they never call each other's internals.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}
