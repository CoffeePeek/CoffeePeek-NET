using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Application.Services;

public interface ISessionManager
{
    Task SignInAsync(UserCredential user, bool isPersistent = false);
}