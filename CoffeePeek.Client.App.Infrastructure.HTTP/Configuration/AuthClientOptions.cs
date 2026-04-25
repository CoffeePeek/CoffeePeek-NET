namespace CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;

/// <summary>
/// Auth-related HTTP paths (relative to the API base address configured in <c>ApiOptions</c>).
/// </summary>
public sealed class AuthClientOptions
{
    /// <summary>Relative path for login (POST), refresh (PUT), and logout (DELETE), e.g. <c>api/Tokens</c>.</summary>
    public string TokensPath { get; set; } = "api/Tokens";
}
