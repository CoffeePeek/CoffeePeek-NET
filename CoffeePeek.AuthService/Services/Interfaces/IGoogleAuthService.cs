using Google.Apis.Auth;

namespace CoffeePeek.AuthService.Services;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}




