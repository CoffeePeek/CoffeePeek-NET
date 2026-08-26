namespace CoffeePeek.Shared.Domain.Places;

public static class ShopPlaceMatcher
{
    public const double DuplicateMeters = 100;
    public const double TightDuplicateMeters = 50;

    public static bool IsSamePlace(
        string? nameA,
        decimal latA,
        decimal lonA,
        string? nameB,
        decimal latB,
        decimal lonB,
        string? phoneA = null,
        string? phoneB = null,
        string? instagramA = null,
        string? instagramB = null)
    {
        var igA = NormalizeInstagram(instagramA);
        var igB = NormalizeInstagram(instagramB);
        if (igA is not null && igB is not null && igA == igB)
            return DistanceMeters(latA, lonA, latB, lonB) <= 500;

        var phoneDigitsA = NormalizePhone(phoneA);
        var phoneDigitsB = NormalizePhone(phoneB);
        if (phoneDigitsA is not null && phoneDigitsB is not null && phoneDigitsA == phoneDigitsB)
            return DistanceMeters(latA, lonA, latB, lonB) <= 500;

        var distance = DistanceMeters(latA, lonA, latB, lonB);
        if (distance > DuplicateMeters)
            return false;

        return NamesMatch(nameA, nameB, distance);
    }

    public static bool NamesMatch(string? nameA, string? nameB, double distanceMeters = 0)
    {
        var a = NormalizeName(nameA);
        var b = NormalizeName(nameB);
        if (a.Length == 0 || b.Length == 0)
            return false;

        if (a == b)
            return true;

        if (distanceMeters > TightDuplicateMeters)
            return false;

        var shorter = a.Length <= b.Length ? a : b;
        var longer = a.Length <= b.Length ? b : a;
        if (shorter.Length >= 6 && longer.Contains(shorter, StringComparison.Ordinal))
            return true;

        return a.Length >= 5 && b.Length >= 5 && Levenshtein(a, b) <= 2;
    }

    public static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        var chars = new char[name.Length];
        var n = 0;
        var prevSpace = true;
        foreach (var raw in name.Trim().ToLowerInvariant())
        {
            var c = raw == 'ё' ? 'е' : raw;
            if (c is '«' or '»' or '"' or '\'' or '`' or '.' or ',' or '-' or '–' or '—'
                or '(' or ')' or '[' or ']' or '!')
                continue;

            if (char.IsWhiteSpace(c))
            {
                if (prevSpace)
                    continue;
                chars[n++] = ' ';
                prevSpace = true;
                continue;
            }

            chars[n++] = c;
            prevSpace = false;
        }

        var normalized = new string(chars, 0, n).Trim();
        const string coffeeShopRu = "кофейня ";
        if (normalized.StartsWith(coffeeShopRu, StringComparison.Ordinal))
            normalized = normalized[coffeeShopRu.Length..].Trim();

        return normalized;
    }

    public static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new char[phone.Length];
        var n = 0;
        foreach (var c in phone)
        {
            if (char.IsDigit(c))
                digits[n++] = c;
        }

        if (n < 7)
            return null;

        var all = new string(digits, 0, n);
        return all.Length > 9 ? all[^9..] : all;
    }

    public static string? NormalizeInstagram(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim().TrimEnd('/');
        var at = trimmed.LastIndexOf('/');
        if (at >= 0 && trimmed.Contains("instagram", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[(at + 1)..];

        trimmed = trimmed.TrimStart('@');
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed.ToLowerInvariant();
    }

    public static double DistanceMeters(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double earthRadius = 6_371_000;
        var phi1 = DegreesToRadians((double)lat1);
        var phi2 = DegreesToRadians((double)lat2);
        var dPhi = DegreesToRadians((double)(lat2 - lat1));
        var dLambda = DegreesToRadians((double)(lon2 - lon1));
        var a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2)
                + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static bool IsGenericAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return true;

        var trimmed = address.Trim();
        return trimmed.Equals("Минск", StringComparison.OrdinalIgnoreCase)
               || trimmed.Equals("Minsk", StringComparison.OrdinalIgnoreCase)
               || trimmed.Equals("Беларусь", StringComparison.OrdinalIgnoreCase)
               || trimmed.Equals("Belarus", StringComparison.OrdinalIgnoreCase);
    }

    public static string? PreferRicherText(string? current, string? incoming)
    {
        if (string.IsNullOrWhiteSpace(incoming))
            return current;
        if (string.IsNullOrWhiteSpace(current))
            return incoming.Trim();

        var currentTrim = current.Trim();
        var incomingTrim = incoming.Trim();
        if (IsGenericAddress(currentTrim) && !IsGenericAddress(incomingTrim))
            return incomingTrim;
        if (incomingTrim.Length > currentTrim.Length
            && incomingTrim.Contains(currentTrim, StringComparison.OrdinalIgnoreCase))
            return incomingTrim;

        return currentTrim;
    }

    private static int Levenshtein(string a, string b)
    {
        if (a == b)
            return 0;
        if (a.Length == 0)
            return b.Length;
        if (b.Length == 0)
            return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
            prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }

            (prev, curr) = (curr, prev);
        }

        return prev[b.Length];
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
