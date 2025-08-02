using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Core.Generated;

public static class UnitOfWorkServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratedUnitOfWorkWrappers(
        this IServiceCollection services,
        params Assembly[]? assemblies)
    {
        var scanAssemblies = (assemblies is { Length: > 0 })
            ? assemblies
            : AppDomain.CurrentDomain.GetAssemblies()
                      .Where(a => !a.IsDynamic && !a.FullName!.StartsWith("Microsoft"))
                      .ToArray();

        foreach (var asm in scanAssemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex)
            { types = ex.Types!.Where(t => t is not null).Cast<Type>().ToArray(); }

            foreach (var wrapper in types.Where(t =>
                         t.IsClass && !t.IsAbstract && t.Name.EndsWith("_UowWrapper")))
            {
                foreach (var iface in wrapper.GetInterfaces())
                {
                    if (IsRepositoryInterface(iface))
                    {
                        services.AddTransient(iface, wrapper);
                    }
                }
            }
        }

        return services;
    }

    // ——— helpers ———
    private static bool IsRepositoryInterface(Type type)
    {
        // 1. direct IRepository<,>
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IRepository<,>))
            return true;

        // 2. *inherits* from IRepository<,>
        return type.GetInterfaces().Any(IsRepositoryInterface);
    }
}
