namespace ShipMvp.Core.Attributes;

// Auto-controller generation
[AttributeUsage(AttributeTargets.Class)]
public class AutoControllerAttribute : Attribute
{
    public string? Route { get; set; }
}
