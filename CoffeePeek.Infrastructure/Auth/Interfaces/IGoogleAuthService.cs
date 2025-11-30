using Google.Apis.Auth;

namespace CoffeePeek.Infrastructure.Auth;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken);
}