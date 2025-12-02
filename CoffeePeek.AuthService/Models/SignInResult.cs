namespace CoffeePeek.AuthService.Models;

public enum SignInResult
{
    Success,
    Failed,
    RequiresTwoFactor,
    NotAllowed
}