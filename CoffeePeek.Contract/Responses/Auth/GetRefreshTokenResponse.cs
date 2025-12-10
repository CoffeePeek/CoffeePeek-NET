namespace CoffeePeek.Contract.Response.Auth;

public class GetRefreshTokenResponse
{
    public GetRefreshTokenResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public string AccessToken { get; }
    public string RefreshToken { get; }
}