using DataForge.BuildingBlocks.Application.Events.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataForge.BuildingBlocks.Infrastructure.Events;

/// <summary>
/// Dispatches integration events to handlers in the same process and scope.
/// Handlers run sequentially; a failing handler fails the publishing operation, which keeps the
/// behaviour predictable for the job retry mechanism. A transactional outbox is the documented
/// next step when modules are split into separate deployables (see ADR-0003).
/// </summary>
public sealed partial class InProcessIntegrationEventPublisher(
    IServiceProvider serviceProvider,
    ILogger<InProcessIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        foreach (var handler in serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>())
        {
            LogDispatching(logger, typeof(TEvent).Name, integrationEvent.EventId, handler.GetType().Name);
            await handler.HandleAsync(integrationEvent, cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dispatching {EventName} {EventId} to {HandlerName}")]
    private static partial void LogDispatching(ILogger logger, string eventName, Guid eventId, string handlerName);
}
