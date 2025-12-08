using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Services;

public interface ISignInManager
{
    Task<SignInResultWrapper> CheckPasswordSignInAsync(UserCredentials user, string requestPassword);
}