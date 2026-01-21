using CoffeePeek.Account.Application.Common.Models;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, string device, string ip);
}