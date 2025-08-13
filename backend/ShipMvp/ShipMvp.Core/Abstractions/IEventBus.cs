namespace ShipMvp.Core.Abstractions;

// --- Event Bus Abstractions ---
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default);
}
