namespace CoffeePeek.Client.App.Services;

/// <summary>
/// Routes profile open/close without injecting <c>WorkspaceViewModel</c> into <c>UserProfileViewModel</c>
/// (breaks an Autofac circular dependency).
/// </summary>
public sealed class WorkspaceShellNavigator : IWorkspaceShellNavigator
{
    private Action<Guid>? _openProfile;
    private Action? _closeProfile;
    private Action<Guid>? _openShopDetail;
    private Action? _closeShopDetail;
    private Action? _openSuggestShop;
    private Action? _closeSuggestShop;

    public void AttachProfile(Action<Guid> open, Action close)
    {
        _openProfile = open;
        _closeProfile = close;
    }

    public void AttachShopDetail(Action<Guid> open, Action close)
    {
        _openShopDetail = open;
        _closeShopDetail = close;
    }

    public void AttachSuggestShop(Action open, Action close)
    {
        _openSuggestShop = open;
        _closeSuggestShop = close;
    }

    public void OpenUserProfile(Guid userId) => _openProfile?.Invoke(userId);

    public void CloseUserProfile() => _closeProfile?.Invoke();

    public void OpenShopDetail(Guid shopId) => _openShopDetail?.Invoke(shopId);

    public void CloseShopDetail() => _closeShopDetail?.Invoke();

    public void OpenSuggestShop() => _openSuggestShop?.Invoke();

    public void CloseSuggestShop() => _closeSuggestShop?.Invoke();
}
