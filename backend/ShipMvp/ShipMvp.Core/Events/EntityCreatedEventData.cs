namespace ShipMvp.Core.Events;

public class EntityCreatedEventData<TEntity>
{
    public TEntity Entity { get; }
    public EntityCreatedEventData(TEntity entity) => Entity = entity;
}
