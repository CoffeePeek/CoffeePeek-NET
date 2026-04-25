namespace CoffeePeek.Client.App.Services;

public interface IWorkspaceShellNavigator
{
    void OpenUserProfile(Guid userId);

    void CloseUserProfile();

    void OpenShopDetail(Guid shopId);

    void CloseShopDetail();
}
