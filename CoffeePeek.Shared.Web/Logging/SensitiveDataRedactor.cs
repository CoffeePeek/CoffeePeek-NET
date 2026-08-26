using System.Text.RegularExpressions;

namespace CoffeePeek.Shared.Web.Logging;

/// <summary>
/// Removes credentials from log text so query-string JWTs and similar secrets
/// never reach the console, files, or error trackers.
/// </summary>
public static partial class SensitiveDataRedactor
{
    [GeneratedRegex(
        @"([?&](access_token|refresh_token|id_token|token|password|secret|api_key|authorization)=)[^&]*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SecretQueryParameterRegex();

    [GeneratedRegex(
        @"(Bearer\s+)[A-Za-z0-9\-._~+/]+=*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+")]
    private static partial Regex JwtRegex();

    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var redacted = SecretQueryParameterRegex().Replace(value, "$1[REDACTED]");
        redacted = BearerTokenRegex().Replace(redacted, "$1[REDACTED]");
        return JwtRegex().Replace(redacted, "[REDACTED_JWT]");
    }
}
