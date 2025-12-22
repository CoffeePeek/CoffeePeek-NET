using Google.Apis.Auth;

namespace CoffeePeek.Account.Application.Services;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}