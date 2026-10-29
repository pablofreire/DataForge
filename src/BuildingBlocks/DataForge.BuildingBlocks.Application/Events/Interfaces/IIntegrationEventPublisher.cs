namespace DataForge.BuildingBlocks.Application.Events.Interfaces;

/// <summary>
/// Publishes integration events to every registered handler. The in-process implementation can be swapped for
/// an outbox + message broker without changing publishers or handlers.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent;
}
