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

    // Extra properties (for extensibility)
    public Dictionary<string, object> ExtraProperties { get; set; } = new();

    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        EntityVersion = 0;
    }

    // Helper for extra properties
    public void SetProperty(string key, object value) => ExtraProperties[key] = value;
    public T? GetProperty<T>(string key) => ExtraProperties.TryGetValue(key, out var value) ? (T)value : default;
    public bool HasProperty(string key) => ExtraProperties.ContainsKey(key);
    public void RemoveProperty(string key) => ExtraProperties.Remove(key);
}
