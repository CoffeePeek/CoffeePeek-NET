namespace CoffeePeek.Contract.Responses.Auth;

public record GetRefreshTokenResponse(string AccessToken, string RefreshToken);
