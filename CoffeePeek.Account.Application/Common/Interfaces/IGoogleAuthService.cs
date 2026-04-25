using Google.Apis.Auth;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken);
}