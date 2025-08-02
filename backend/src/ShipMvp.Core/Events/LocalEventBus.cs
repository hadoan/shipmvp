using ShipMvp.Core.Abstractions;

namespace ShipMvp.Core.Events;

public class LocalEventBus : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
    {
        // No-op for now; extend as needed for real event handling
        return Task.CompletedTask;
    }
}