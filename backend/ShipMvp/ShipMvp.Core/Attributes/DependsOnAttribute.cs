using ShipMvp.Core.Modules;

namespace ShipMvp.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute<T> : Attribute where T : IModule { }
