using Autofac;
using CoffeePeek.Client.App.Core.Execution;

namespace CoffeePeek.Client.App.Execution;

public sealed class ApplicationExecutorRunner(ILifetimeScope scope) : IApplicationExecutorRunner
{
    public Task RunAfterInitAsync(CancellationToken cancellationToken = default) =>
        RunStageAsync<IAfterInitExecutor>(cancellationToken);

    public Task RunBeforeMainShellAsync(CancellationToken cancellationToken = default) =>
        RunStageAsync<IBeforeMainShellExecutor>(cancellationToken);

    public Task RunAfterStartupAsync(CancellationToken cancellationToken = default) =>
        RunStageAsync<IAfterStartupExecutor>(cancellationToken);

    private async Task RunStageAsync<TExecutor>(CancellationToken cancellationToken)
        where TExecutor : IApplicationExecutor
    {
        var executors = scope.Resolve<IEnumerable<TExecutor>>().OrderBy(e => e.Order);
        foreach (var executor in executors)
            await executor.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}