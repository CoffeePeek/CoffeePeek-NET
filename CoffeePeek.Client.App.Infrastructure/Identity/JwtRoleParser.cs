using System.Text;
using System.Text.Json;

namespace CoffeePeek.Client.App.Infrastructure.Identity;

internal static class JwtRoleParser
{
    private const string LongRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    public static IReadOnlyList<string> ReadRoles(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return Array.Empty<string>();

        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return Array.Empty<string>();

        try
        {
            var payload = DecodeUtf8(parts[1]);
            using var doc = JsonDocument.Parse(payload);
            return CollectRolesFrom(doc.RootElement);
        }
        catch (FormatException)
        {
            return Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static IReadOnlyList<string> CollectRolesFrom(JsonElement root)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddRoleIfPresent(root, "role", set);
        AddRoleIfPresent(root, LongRoleClaimType, set);
        return [.. set];
    }

    private static void AddRoleIfPresent(JsonElement root, string property, HashSet<string> list)
    {
        if (!root.TryGetProperty(property, out var el))
            return;

        switch (el.ValueKind)
        {
            case JsonValueKind.String:
                var s = el.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    list.Add(s!);
                return;
            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var role = item.GetString();
                        if (!string.IsNullOrWhiteSpace(role))
                            list.Add(role!);
                    }
                }
                return;
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
