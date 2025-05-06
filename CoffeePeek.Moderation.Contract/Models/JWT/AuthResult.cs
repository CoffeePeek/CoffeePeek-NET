namespace CoffeePeek.Moderation.Contract.Models.JWT;

public record AuthResult(string AccessToken, string RefreshToken, DateTime ExpireDate)
{
    public string AccessToken { get; set; } = AccessToken;
    public string RefreshToken { get; set; } = RefreshToken;
    public DateTime ExpireDate { get; set; } = ExpireDate;
}