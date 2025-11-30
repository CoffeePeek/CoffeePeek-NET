using CoffeePeek.BuildingBlocks.AuthOptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Infrastructure.Auth;

public class GoogleAuthService(IOptions<OAuthGoogleOptions> options) : IGoogleAuthService
{
    public async Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [options.Value.ClientId]
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch
        {
            return null;
        }
    }
}