namespace CoffeePeek.Client.App.Core.Execution;

/// <summary>
/// Ordered async step in the application lifecycle pipeline.
/// </summary>
public interface IApplicationExecutor
{
    /// <summary>
    /// Lower values run first within the same stage.
    /// </summary>
    int Order { get; }

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}