using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Attributes;

namespace ShipMvp.Core.Modules;

/// <summary>
/// Lean module loader inspired by ABP but simpler
/// Discovers and loads modules with dependency resolution
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// Discovers and registers modules from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for modules</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddModules(this IServiceCollection services, params Assembly[] assemblies)
    {
        var modules = DiscoverModules(assemblies);
        var sortedModules = TopologicalSort(modules);
        
        foreach (var module in sortedModules)
        {
            var instance = Activator.CreateInstance(module) as IModule;
            instance?.ConfigureServices(services);
        }
        
        // Store modules for later configuration
        services.AddSingleton(sortedModules);
        
        return services;
    }
    
    /// <summary>
    /// Configures all registered modules in the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="env">The hosting environment</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder ConfigureModules(this IApplicationBuilder app, IHostEnvironment env)
    {
        var modules = app.ApplicationServices.GetRequiredService<List<Type>>();
        
        foreach (var moduleType in modules)
        {
            var instance = Activator.CreateInstance(moduleType) as IModule;
            instance?.Configure(app, env);
        }
        
        return app;
    }
    
    /// <summary>
    /// Discovers modules in the specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>List of module types</returns>
    private static List<Type> DiscoverModules(Assembly[] assemblies)
    {
        var modules = new List<Type>();
        
        foreach (var assembly in assemblies)
        {
            var moduleTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && 
                           typeof(IModule).IsAssignableFrom(t) &&
                           t.GetCustomAttribute<ModuleAttribute>() != null)
                .ToList();
                
            modules.AddRange(moduleTypes);
        }
        
        return modules;
    }
    
    /// <summary>
    /// Sorts modules based on their dependencies using topological sort
    /// </summary>
    /// <param name="modules">Modules to sort</param>
    /// <returns>Sorted list of modules</returns>
    private static List<Type> TopologicalSort(List<Type> modules)
    {
        var visited = new HashSet<Type>();
        var result = new List<Type>();
        
        foreach (var module in modules)
        {
            if (!visited.Contains(module))
            {
                Visit(module, modules, visited, result);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Visits a module and its dependencies during topological sort
    /// </summary>
    /// <param name="module">Module to visit</param>
    /// <param name="allModules">All available modules</param>
    /// <param name="visited">Set of visited modules</param>
    /// <param name="result">Result list</param>
    private static void Visit(Type module, List<Type> allModules, HashSet<Type> visited, List<Type> result)
    {
        if (visited.Contains(module)) return;
        
        visited.Add(module);
        
        // Get dependencies
        var dependsOnAttributes = module.GetCustomAttributes<DependsOnAttribute<IModule>>();
        foreach (var attr in dependsOnAttributes)
        {
            var depType = attr.GetType().GetGenericArguments().FirstOrDefault();
            if (depType != null && allModules.Contains(depType))
            {
                Visit(depType, allModules, visited, result);
            }
        }
        
        result.Add(module);
    }
}
