using CoffeePeek.BuildingBlocks.AuthOptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeePeek.AuthService.Services;

public class GoogleAuthService(
    IOptions<OAuthGoogleOptions> options,
    ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    private readonly OAuthGoogleOptions _options = options.Value;

    public async Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            logger.LogWarning("Google ID token is null or empty");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            logger.LogError("Google OAuth ClientId is not configured");
            return null;
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            logger.LogInformation("Google ID token validated successfully for user: {Email}", payload.Email);
            
            return payload;
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Invalid Google ID token: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Google ID token");
            return null;
        }
    }
}




