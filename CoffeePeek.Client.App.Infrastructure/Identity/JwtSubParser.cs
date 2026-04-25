using System.Text;
using System.Text.Json;

namespace CoffeePeek.Client.App.Infrastructure.Identity;

internal static class JwtSubParser
{
    public static bool TryGetSubGuid(string? jwt, out Guid userId)
    {
        userId = default;
        if (string.IsNullOrWhiteSpace(jwt))
            return false;

        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return false;

        try
        {
            var payload = DecodeUtf8(parts[1]);
            using var doc = JsonDocument.Parse(payload);
            if (!doc.RootElement.TryGetProperty("sub", out var sub))
                return false;
            var s = sub.GetString();
            return s is not null && Guid.TryParse(s, out userId);
        }
        catch
        {
            return false;
        }
    }

    private static string DecodeUtf8(string segment)
    {
        var padded = PadBase64Url(segment);
        var bytes = Convert.FromBase64String(padded);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string PadBase64Url(string s)
    {
        var sb = new StringBuilder(s.Length + 3);
        sb.Append(s.Replace('-', '+').Replace('_', '/'));
        var mod = sb.Length % 4;
        if (mod > 0)
            sb.Append('=', 4 - mod);
        return sb.ToString();
    }
}
