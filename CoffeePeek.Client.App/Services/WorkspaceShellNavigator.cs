namespace CoffeePeek.Client.App.Services;

/// <summary>
/// Routes shell overlays (user profile, shop detail, suggest-shop, settings, moderation) without
/// injecting <c>WorkspaceViewModel</c> into feature view models (avoids Autofac circular dependencies).
/// </summary>
public sealed class WorkspaceShellNavigator : IWorkspaceShellNavigator
{
    private Action<Guid>? _openProfile;
    private Action? _closeProfile;
    private Action<Guid>? _openShopDetail;
    private Action? _closeShopDetail;
    private Action? _openSuggestShop;
    private Action? _closeSuggestShop;
    private Action? _openSettings;
    private Action? _closeSettings;
    private Action? _openModerationPanel;
    private Action? _closeModerationPanel;

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

    public void AttachSettings(Action open, Action close)
    {
        _openSettings = open;
        _closeSettings = close;
    }

    public void AttachModerationPanel(Action open, Action close)
    {
        _openModerationPanel = open;
        _closeModerationPanel = close;
    }

    public void OpenUserProfile(Guid userId) => _openProfile?.Invoke(userId);

    public void CloseUserProfile() => _closeProfile?.Invoke();

    public void OpenShopDetail(Guid shopId) => _openShopDetail?.Invoke(shopId);

    public void CloseShopDetail() => _closeShopDetail?.Invoke();

    public void OpenSuggestShop() => _openSuggestShop?.Invoke();

    public void CloseSuggestShop() => _closeSuggestShop?.Invoke();

    public void OpenSettings() => _openSettings?.Invoke();

    public void CloseSettings() => _closeSettings?.Invoke();

    public void OpenModerationPanel() => _openModerationPanel?.Invoke();

    public void CloseModerationPanel() => _closeModerationPanel?.Invoke();
}
