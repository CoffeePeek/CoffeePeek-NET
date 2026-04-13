namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses.Auth;

/// <summary>Matches server <c>RefreshTokenResponse</c> inside API envelope <c>data</c>.</summary>
public sealed class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}
