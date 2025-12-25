using CoffeePeek.Contract.Dtos.Auth;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, string device, string ip);
}