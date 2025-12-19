namespace CoffeePeek.Auth.Application.Models;

public enum SignInResult
{
    Success,
    Failed,
    RequiresTwoFactor,
    NotAllowed
}