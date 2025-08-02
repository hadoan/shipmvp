using Microsoft.Extensions.DependencyInjection;

namespace ShipMvp.CLI.Commands;

public class CommandRunner : ICommandRunner
{
    private readonly IServiceProvider _serviceProvider;

    public CommandRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task ExecuteAsync<T>(string[] args, CancellationToken cancellationToken = default) where T : ICommand
    {
        var command = _serviceProvider.GetRequiredService<T>();
        await command.ExecuteAsync(args, cancellationToken);
    }
}
