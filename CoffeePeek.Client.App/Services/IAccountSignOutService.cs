namespace CoffeePeek.Client.App.Services;

public interface IAccountSignOutService
{
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
