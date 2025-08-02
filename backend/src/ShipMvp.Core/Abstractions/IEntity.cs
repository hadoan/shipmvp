namespace ShipMvp.Core.Entities;

// Core abstractions for lean architecture
public interface IEntity<TId>
{
    TId Id { get; }
}
