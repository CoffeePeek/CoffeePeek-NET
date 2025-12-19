using Google.Apis.Auth;

namespace CoffeePeek.Auth.Application.Services;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}