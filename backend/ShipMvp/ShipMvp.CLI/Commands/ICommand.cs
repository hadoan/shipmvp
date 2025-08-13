namespace ShipMvp.CLI.Commands;

public interface ICommand
{
    Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default);
}

public interface ICommandRunner
{
    Task ExecuteAsync<T>(string[] args, CancellationToken cancellationToken = default) where T : ICommand;
}
