namespace ShipMvp.Core.Attributes;

// Auto-mapping
[AttributeUsage(AttributeTargets.Class)]
public class AutoMapAttribute : Attribute
{
    public Type[] Types { get; }
    
    public AutoMapAttribute(params Type[] types)
    {
        Types = types;
    }
}
