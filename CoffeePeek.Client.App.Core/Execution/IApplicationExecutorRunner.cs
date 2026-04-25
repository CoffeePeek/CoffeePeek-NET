namespace CoffeePeek.Client.App.Core.Execution;

public interface IApplicationExecutorRunner
{
    Task RunAfterInitAsync(CancellationToken cancellationToken = default);

    Task RunBeforeMainShellAsync(CancellationToken cancellationToken = default);

    Task RunAfterStartupAsync(CancellationToken cancellationToken = default);
}