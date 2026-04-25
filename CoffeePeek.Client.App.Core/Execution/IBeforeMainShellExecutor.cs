namespace CoffeePeek.Client.App.Core.Execution;

/// <summary>
/// Runs after Avalonia framework init is completing but before the main window / root view is created.
/// Use for session restore, auth checks, and choosing the initial navigation target.
/// </summary>
public interface IBeforeMainShellExecutor : IApplicationExecutor;
