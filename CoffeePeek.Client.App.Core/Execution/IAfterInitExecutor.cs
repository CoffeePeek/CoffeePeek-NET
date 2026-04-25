namespace CoffeePeek.Client.App.Core.Execution;

/// <summary>
/// Runs after the DI container is built and application <c>Initialize</c> has assigned the container
/// (e.g. after XAML load and <c>Bootstrapper.BuildContainer</c>).
/// </summary>
public interface IAfterInitExecutor : IApplicationExecutor;