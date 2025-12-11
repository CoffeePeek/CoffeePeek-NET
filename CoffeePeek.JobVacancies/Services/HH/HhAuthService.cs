using CoffeePeek.JobVacancies.Configuration;
using CoffeePeek.JobVacancies.Models.Responses;
using Microsoft.Extensions.Options;

namespace CoffeePeek.JobVacancies.Services;

public class HhAuthService(HttpClient httpClient, IOptions<HhApiOptions> options) : IHhAuthService
{
    private readonly HhApiOptions _options = options.Value;
    private HhToken? _token;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_token == null || DateTime.UtcNow >= _token.IssuedAt + TimeSpan.FromSeconds(_token.ExpiresIn))
        {
            if (_token?.RefreshToken != null)
            {
                _token = await RefreshTokenAsync(_token.RefreshToken, cancellationToken);
            }
            else
            {
                _token = await RequestTokenAsync(cancellationToken);
            }
        }

        return _token.AccessToken;
    }

    private async Task<HhToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = _options.Code,
            ["redirect_uri"] = _options.RedirectUri
        });

        using var response = await httpClient.PostAsync("token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<HhToken>(cancellationToken: cancellationToken)
                    ?? throw new Exception("Failed to deserialize token");

        token.IssuedAt = DateTime.UtcNow;
        return token;
    }

    private async Task<HhToken> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        using var response = await httpClient.PostAsync("token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<HhToken>(cancellationToken: cancellationToken)
                    ?? throw new Exception("Failed to deserialize refreshed token");

        token.IssuedAt = DateTime.UtcNow;
        return token;
    }
}