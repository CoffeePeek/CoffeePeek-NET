namespace CoffeePeek.Account.Application.Common.Models;

public class AuthResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
}