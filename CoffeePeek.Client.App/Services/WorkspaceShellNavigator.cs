namespace CoffeePeek.Client.App.Services;

/// <summary>
/// Routes profile open/close without injecting <c>WorkspaceViewModel</c> into <c>UserProfileViewModel</c>
/// (breaks an Autofac circular dependency).
/// </summary>
public sealed class WorkspaceShellNavigator : IWorkspaceShellNavigator
{
    private Action<Guid>? _open;

    private Action? _close;

    public void Attach(Action<Guid> open, Action close)
    {
        _open = open;
        _close = close;
    }

    public void OpenUserProfile(Guid userId) => _open?.Invoke(userId);

    public void CloseUserProfile() => _close?.Invoke();
}
