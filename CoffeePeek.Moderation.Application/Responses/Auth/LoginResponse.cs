namespace CoffeePeek.Moderation.Application.Responses;

public record LoginResponse(string AccessToken, string RefreshToken)
{
    public string AccessToken { get; set; } = AccessToken;
    public string RefreshToken { get; set; } = RefreshToken;
}