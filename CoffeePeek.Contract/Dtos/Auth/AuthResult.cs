namespace CoffeePeek.Contract.Dtos.Auth;

public class AuthResult
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiredAt { get; set; }
}