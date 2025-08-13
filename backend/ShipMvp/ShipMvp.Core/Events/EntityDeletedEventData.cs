namespace ShipMvp.Core.Events;

public class EntityDeletedEventData<TEntity>
{
    public TEntity Entity { get; }
    public EntityDeletedEventData(TEntity entity) => Entity = entity;
}
