using CoffeePeek.AuthService.Entities;

namespace CoffeePeek.AuthService.Services;

public interface ISessionManager
{
    Task SignInAsync(UserCredentials user, bool isPersistent = false);
}