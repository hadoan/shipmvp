namespace ShipMvp.Core.Entities;

public abstract class Entity<TId> : IEntity<TId>
{
    public TId Id { get; protected set; }

    // Auditing
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Versioning
    public int EntityVersion { get; set; }

    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        EntityVersion = 0;
    }
}
