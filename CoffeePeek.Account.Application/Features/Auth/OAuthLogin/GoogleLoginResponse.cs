namespace CoffeePeek.Account.Application.Features.Auth.OAuthLogin;

public class GoogleLoginResponse
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
    public GoogleLoginUser User { get; set; }
}