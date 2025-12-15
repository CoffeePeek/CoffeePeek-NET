namespace CoffeePeek.Contract.Responses.Login;

public class LoginResponse(string accessToken, string refreshToken)
{
    public string AccessToken { get; init; } = accessToken;
    public string RefreshToken { get; init; } = refreshToken;
}