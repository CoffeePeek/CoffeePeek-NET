using FluentResults;

namespace CoffeePeek.Client.App.Services;

public interface IAccountSignOutService
{
    Task<Result> SignOutAsync(CancellationToken cancellationToken = default);
}
