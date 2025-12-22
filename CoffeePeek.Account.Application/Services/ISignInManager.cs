using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Application.Models;

namespace CoffeePeek.Account.Application.Services;

public interface ISignInManager
{
    Task<SignInResultWrapper> CheckPasswordSignInAsync(UserCredential user, string requestPassword);
}