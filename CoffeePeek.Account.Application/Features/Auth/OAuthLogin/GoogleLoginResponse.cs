namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public class GoogleLoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required GoogleLoginUser User { get; init; }
}
