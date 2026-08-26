using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Moderation.Domain.Import;

public static class ImportContactNormalizer
{
    private static readonly HashSet<string> BlockedInstagramPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "reel", "reels", "stories", "explore", "accounts", "direct"
    };

    public static string? Instagram(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var trimmed = raw.Trim();
        if (trimmed.StartsWith('@'))
            trimmed = trimmed[1..].Trim();

        Uri uri;
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            uri = absolute;
        }
        else if (trimmed.StartsWith("instagram.com", StringComparison.OrdinalIgnoreCase)
                 || trimmed.StartsWith("www.instagram.com", StringComparison.OrdinalIgnoreCase))
        {
            uri = new Uri("https://" + trimmed.TrimStart('/'));
        }
        else if (IsProfileHandle(trimmed))
        {
            uri = new Uri("https://www.instagram.com/" + trimmed.Trim('/') + "/");
        }
        else
        {
            throw new DomainException("Instagram must be a profile URL or @handle.");
        }

        var host = uri.Host.TrimStart().ToLowerInvariant();
        if (host is not ("instagram.com" or "www.instagram.com"))
            throw new DomainException("Instagram URL must be on instagram.com.");

        var handle = uri.AbsolutePath.Trim('/').Split('/', 2, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(handle) || BlockedInstagramPaths.Contains(handle))
            throw new DomainException("Instagram URL must be a profile, not a post or explore page.");

        if (handle.Length > 30 || handle.Contains('?'))
            throw new DomainException("Instagram handle is invalid.");

        return $"https://www.instagram.com/{handle}/";
    }

    public static string? Website(string? raw, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var trimmed = raw.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new DomainException("Website must be an http or https URL.");

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static bool IsProfileHandle(string value)
    {
        if (value.Length is 0 or > 30 || value.Contains('/') || value.Contains(' '))
            return false;

        if (!value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_'))
            return false;

        return !value.EndsWith(".com", StringComparison.OrdinalIgnoreCase)
               && !value.EndsWith(".by", StringComparison.OrdinalIgnoreCase)
               && !value.EndsWith(".ru", StringComparison.OrdinalIgnoreCase)
               && !value.EndsWith(".net", StringComparison.OrdinalIgnoreCase)
               && !value.EndsWith(".org", StringComparison.OrdinalIgnoreCase)
               && !value.EndsWith(".io", StringComparison.OrdinalIgnoreCase);
    }
}
