using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipMvp.Application;

using ShipMvp.CLI.Commands;
using ShipMvp.Core;
using ShipMvp.Core.Generated;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Modules;
namespace ShipMvp.CLI;

/// <summary>
/// CLI Module for configuring command-line interface services and dependencies
/// </summary>
[Module]
[DependsOn<ApplicationModule>]
public class CLIModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register CLI commands
        services.AddScoped<SeedIntegrationsCommand>();
        services.AddScoped<SeedDataCommand>();
        services.AddScoped<RunSqlCommand>();
        
        // Register command factory/resolver
        services.AddScoped<ICommandResolver, CommandResolver>();
        
        // CLI-specific services
        services.AddScoped<ICliOutputFormatter, CliOutputFormatter>();

        services.AddGeneratedUnitOfWorkWrappers();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        // CLI doesn't need HTTP pipeline configuration
        // This is here for interface compliance
        var logger = app.ApplicationServices.GetRequiredService<ILogger<CLIModule>>();
        logger.LogInformation("CLI Module: Command-line interface configured successfully");
    }
}

/// <summary>
/// Command resolver for mapping command names to implementations
/// </summary>
public interface ICommandResolver
{
    Task<bool> ExecuteCommandAsync(string commandName, string[] args, CancellationToken cancellationToken = default);
    IEnumerable<string> GetAvailableCommands();
}

public class CommandResolver : ICommandResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandResolver> _logger;
    
    private readonly Dictionary<string, Type> _commandMap = new()
    {
        ["seed-integrations"] = typeof(SeedIntegrationsCommand),
        ["seed-data"] = typeof(SeedDataCommand),
        ["run-sql"] = typeof(RunSqlCommand)
    };

    public CommandResolver(IServiceProvider serviceProvider, ILogger<CommandResolver> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> ExecuteCommandAsync(string commandName, string[] args, CancellationToken cancellationToken = default)
    {
        if (!_commandMap.TryGetValue(commandName, out var commandType))
        {
            _logger.LogError("Unknown command: {CommandName}", commandName);
            ShowHelp();
            return false;
        }

        try
        {
            _logger.LogInformation("Executing command: {CommandName}", commandName);
            
            var command = _serviceProvider.GetRequiredService(commandType) as ICommand;
            if (command == null)
            {
                _logger.LogError("Failed to resolve command: {CommandName}", commandName);
                return false;
            }

            await command.ExecuteAsync(args, cancellationToken);
            _logger.LogInformation("Command completed successfully: {CommandName}", commandName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {CommandName}", commandName);
            return false;
        }
    }

    public IEnumerable<string> GetAvailableCommands() => _commandMap.Keys;

    private void ShowHelp()
    {
        Console.WriteLine("ShipMvp CLI Tool");
        Console.WriteLine("================");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  seed-integrations    Seed integration platforms from appsettings.json");
        Console.WriteLine("  seed-data           Seed initial application data (users, plans, etc.)");
        Console.WriteLine("  run-sql             Execute a SQL query against the database");
        Console.WriteLine("  help                Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run seed-integrations");
        Console.WriteLine("  dotnet run seed-data");
        Console.WriteLine("  dotnet run run-sql \"SELECT * FROM Integrations\"");
        Console.WriteLine("  dotnet run help");
        Console.WriteLine();
    }
}

/// <summary>
/// CLI output formatter for consistent command output
/// </summary>
public interface ICliOutputFormatter
{
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteWarning(string message);
    void WriteInfo(string message);
    void WriteTable<T>(IEnumerable<T> data);
}

public class CliOutputFormatter : ICliOutputFormatter
{
    public void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    public void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {message}");
        Console.ResetColor();
    }

    public void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {message}");
        Console.ResetColor();
    }

    public void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }

    public void WriteTable<T>(IEnumerable<T> data)
    {
        // Simple table formatting - can be enhanced later
        foreach (var item in data)
        {
            Console.WriteLine(item?.ToString());
        }
    }
}
