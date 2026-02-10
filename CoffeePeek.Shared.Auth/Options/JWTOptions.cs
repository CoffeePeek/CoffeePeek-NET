namespace CoffeePeek.Shared.Auth.Options;

public class JWTOptions
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays { get; set; } 
}