using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShipMvp.CLI;
using ShipMvp.Application;
using ShipMvp.Core.Modules;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

var builder = Host.CreateDefaultBuilder(args);

// Force Development environment for CLI
builder.UseEnvironment(Environments.Development);

// Configure logging
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Configure services using the module system (similar to API)
builder.ConfigureServices((context, services) =>
{
     // Add data protection services required by GoogleAuthAppService
     services.AddDataProtection();
     
     services.AddModules(
            typeof(CLIModule).Assembly,
            typeof(ApplicationModule).Assembly,

            typeof(Program).Assembly);

});

var host = builder.Build();

// Get configuration and log connection string
var configuration = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var connectionString = configuration.GetConnectionString("DefaultConnection");
logger.LogInformation("[ShipMvp CLI] Using connection string: {ConnectionString}", connectionString);

// Run migrations before anything else
using (var migrationScope = host.Services.CreateScope())
{
    var dbContext = migrationScope.ServiceProvider.GetService<ShipMvp.Application.Infrastructure.Data.AppDbContext>();
    if (dbContext != null)
    {
        logger.LogInformation("[ShipMvp CLI] Running database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("[ShipMvp CLI] Database migrations completed.");
    }
    else
    {
        logger.LogWarning("[ShipMvp CLI] AppDbContext not found. Skipping migrations.");
    }
}

// Get command resolver and execute command
var scope = host.Services.CreateScope();
var commandResolver = scope.ServiceProvider.GetRequiredService<ICommandResolver>();
logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    if (args.Length == 0)
    {
        ShowUsage(commandResolver);
        return 1;
    }

    var commandName = args[0];
    var commandArgs = args.Skip(1).ToArray();

    if (commandName == "help" || commandName == "--help" || commandName == "-h")
    {
        ShowUsage(commandResolver);
        return 0;
    }

    logger.LogInformation("ShipMvp CLI starting command: {CommandName}", commandName);
    
    var success = await commandResolver.ExecuteCommandAsync(commandName, commandArgs);
    
    if (success)
    {
        logger.LogInformation("Command completed successfully");
        return 0;
    }
    else
    {
        logger.LogError("Command failed");
        return 1;
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Unhandled exception occurred");
    return 1;
}
finally
{
    scope.Dispose();
    host.Dispose();
}

static void ShowUsage(ICommandResolver commandResolver)
{
    Console.WriteLine("ShipMvp CLI Tool");
    Console.WriteLine("================");
    Console.WriteLine();
    Console.WriteLine("Usage: dotnet run <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    
    foreach (var command in commandResolver.GetAvailableCommands())
    {
        var description = command switch
        {
            "seed-integrations" => "Seed integration platforms from appsettings.json",
            "seed-data" => "Seed initial application data (users, plans, etc.)",
            "run-sql" => "Execute a SQL query against the database",
            _ => "Command description not available"
        };
        Console.WriteLine($"  {command,-20} {description}");
    }
    
    Console.WriteLine("  help                Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run seed-integrations");
    Console.WriteLine("  dotnet run seed-data");
    Console.WriteLine("  dotnet run run-sql \"SELECT * FROM Integrations\"");
    Console.WriteLine("  dotnet run help");
    Console.WriteLine();
    Console.WriteLine("When prompted, enter a command or 'q' to quit.");
}

// Make Program class accessible for testing
public partial class Program { }
