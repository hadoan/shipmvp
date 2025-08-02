using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Infrastructure.Data;
using ShipMvp.Core.Persistence;

namespace ShipMvp.CLI.Commands;

public class SeedDataCommand : ICommand
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedDataCommand> _logger;

    public SeedDataCommand(
        IServiceProvider serviceProvider,
        ILogger<SeedDataCommand> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to seed initial application data...");

        try
        {
            // Create a scope to get the DbContext
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

            // Run the data seeder - note: DataSeeder might need to be updated to accept IDbContext
            // For now, we'll cast it back to AppDbContext for the seeder
            if (context is AppDbContext appDbContext)
            {
                await DataSeeder.SeedAsync(appDbContext);
            }
            else
            {
                _logger.LogError("DbContext is not of type AppDbContext, cannot seed data");
                return;
            }

            _logger.LogInformation("Initial application data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding initial application data");
            throw;
        }
    }
}
