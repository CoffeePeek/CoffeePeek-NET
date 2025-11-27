namespace CoffeePeek.BuildingBlocks.AuthOptions;

public class AuthenticationOptions
{
    public string JwtSecretKey { get; init; }
    public int ExpireIntervalMinutes { get; init; }
    public int ExpireRefreshIntervalDays { get; init; }
    public string ValidAudience { get; init; }
    public string ValidIssuer { get; init; }
}