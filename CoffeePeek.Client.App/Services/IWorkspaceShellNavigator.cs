namespace CoffeePeek.Client.App.Services;

public interface IWorkspaceShellNavigator
{
    void AttachProfile(Action<Guid> open, Action close);

    void AttachShopDetail(Action<Guid> open, Action close);

    void AttachSuggestShop(Action open, Action close);

    void OpenUserProfile(Guid userId);

    void CloseUserProfile();

    void OpenShopDetail(Guid shopId);

    void CloseShopDetail();

    void OpenSuggestShop();

    void CloseSuggestShop();

    void OpenSettings();

    void CloseSettings();

    void AttachSettings(Action open, Action close);
}
