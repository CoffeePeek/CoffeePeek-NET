namespace CoffeePeek.Account.Application.Common.Models;

public class AuthResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiredAt { get; set; }
}