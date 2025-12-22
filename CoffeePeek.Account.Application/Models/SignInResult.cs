namespace CoffeePeek.Account.Application.Models;

public enum SignInResult
{
    Success,
    Failed,
    RequiresTwoFactor,
    NotAllowed
}