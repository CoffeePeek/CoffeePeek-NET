namespace CoffeePeek.Shared.Domain.Places;

public readonly record struct PlaceDuplicateHint(
    int Score,
    double DistanceMeters,
    IReadOnlyList<string> Reasons);

/// <summary>
/// Looser than <see cref="ShopPlaceMatcher.IsSamePlace"/> — used to queue admin
/// confirm/reject pairs (e.g. same brand, Belarusian vs Russian street spelling).
/// Never auto-merges.
/// </summary>
public static class PlaceDuplicateSuggester
{
    public const int MinScore = 65;
    public const double MaxMeters = 350;
    public const double NearbyHouseMeters = 180;
    public const double SameNameMeters = 250;

    public static PlaceDuplicateHint? Evaluate(
        string? nameA,
        string? addressA,
        decimal latA,
        decimal lonA,
        string? nameB,
        string? addressB,
        decimal latB,
        decimal lonB,
        string? phoneA = null,
        string? phoneB = null,
        string? instagramA = null,
        string? instagramB = null,
        string? brandA = null,
        string? brandB = null)
    {
        var distance = ShopPlaceMatcher.DistanceMeters(latA, lonA, latB, lonB);
        if (distance > MaxMeters)
            return null;

        var reasons = new List<string>();
        var score = 0;

        if (ShopPlaceMatcher.IsSamePlace(
                nameA, latA, lonA, nameB, latB, lonB, phoneA, phoneB, instagramA, instagramB))
        {
            reasons.Add("strict-match");
            score = Math.Max(score, 96);
        }

        var coreA = CoreName(nameA);
        var coreB = CoreName(nameB);
        var brandCoreA = CoreName(brandA);
        var brandCoreB = CoreName(brandB);

        if (coreA.Length > 0 && coreA == coreB)
        {
            reasons.Add("same-name");
            score = Math.Max(score, distance <= 80 ? 92 : distance <= SameNameMeters ? 84 : 72);
        }
        else if (coreA.Length >= 4 && coreB.Length >= 4 && NamesClose(coreA, coreB))
        {
            reasons.Add("similar-name");
            score = Math.Max(score, distance <= 150 ? 80 : distance <= SameNameMeters ? 70 : 0);
        }

        if (brandCoreA.Length >= 4 && brandCoreA == brandCoreB)
        {
            reasons.Add("same-brand");
            score = Math.Max(score, distance <= SameNameMeters ? 82 : 68);
        }
        else if (SharesBrandToken(coreA, coreB))
        {
            reasons.Add("shared-brand-token");
            score = Math.Max(score, distance <= SameNameMeters ? 74 : 0);
        }

        var houseA = HouseNumber(addressA);
        var houseB = HouseNumber(addressB);
        if (houseA is not null && houseA == houseB && distance <= NearbyHouseMeters)
        {
            reasons.Add("same-house-nearby");
            score = Math.Max(score, score >= 70 ? score + 6 : 70);
        }

        var addrA = NormalizeAddress(addressA);
        var addrB = NormalizeAddress(addressB);
        if (addrA.Length >= 8 && addrB.Length >= 8 && AddressesClose(addrA, addrB))
        {
            reasons.Add("similar-address");
            score = Math.Max(score, coreA.Length > 0 && NamesClose(coreA, coreB) ? 78 : 66);
        }

        score = Math.Min(score, 99);
        if (score < MinScore || reasons.Count == 0)
            return null;

        reasons.Add($"distance:{Math.Round(distance)}m");
        return new PlaceDuplicateHint(score, distance, reasons);
    }

    public static string FoldEastSlavic(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = chars[i] switch
            {
                'і' or 'І' => 'и',
                'ў' or 'Ў' => 'у',
                'ґ' or 'Ґ' => 'г',
                '’' or 'ʻ' or '´' or '`' => '\'',
                _ => chars[i]
            };
        }

        return new string(chars);
    }

    public static string CoreName(string? name)
    {
        var normalized = ShopPlaceMatcher.NormalizeName(FoldEastSlavic(name ?? ""));
        foreach (var prefix in new[] { "кафе ", "cafe ", "кофе ", "кавярня ", "кава " })
        {
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                normalized = normalized[prefix.Length..].Trim();
        }

        return normalized;
    }

    public static string NormalizeAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return "";

        var folded = FoldEastSlavic(address.ToLowerInvariant());
        folded = folded
            .Replace("праспект", "пр", StringComparison.Ordinal)
            .Replace("проспект", "пр", StringComparison.Ordinal)
            .Replace("пр-т", "пр", StringComparison.Ordinal)
            .Replace("вуліца", "ул", StringComparison.Ordinal)
            .Replace("улица", "ул", StringComparison.Ordinal)
            .Replace("вул.", "ул", StringComparison.Ordinal)
            .Replace("ул.", "ул", StringComparison.Ordinal)
            .Replace("плошча", "пл", StringComparison.Ordinal)
            .Replace("площадь", "пл", StringComparison.Ordinal)
            .Replace("завулак", "пер", StringComparison.Ordinal)
            .Replace("переулок", "пер", StringComparison.Ordinal);

        var normalized = ShopPlaceMatcher.NormalizeName(folded);
        foreach (var noise in new[] { "минск", "мінск", "belarus", "беларусь", "беларус" })
            normalized = normalized.Replace(noise, " ", StringComparison.Ordinal);

        return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string? HouseNumber(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        string? last = null;
        var current = new char[16];
        var n = 0;
        foreach (var c in address)
        {
            if (char.IsDigit(c) || (n > 0 && (c is '/' or '\\' or '-' or 'а' or 'б' or 'в' or 'a' or 'b' or 'c')))
            {
                if (n < current.Length)
                    current[n++] = char.ToLowerInvariant(c);
            }
            else if (n > 0)
            {
                var token = new string(current, 0, n);
                if (token.Any(char.IsDigit))
                    last = token;
                n = 0;
            }
        }

        if (n > 0)
        {
            var token = new string(current, 0, n);
            if (token.Any(char.IsDigit))
                last = token;
        }

        return last;
    }

    private static bool NamesClose(string a, string b)
    {
        if (a == b)
            return true;
        // Distance 0 unlocks the contains / Levenshtein branch inside NamesMatch.
        if (ShopPlaceMatcher.NamesMatch(a, b, distanceMeters: 0))
            return true;

        var shorter = a.Length <= b.Length ? a : b;
        var longer = a.Length <= b.Length ? b : a;
        return shorter.Length >= 6 && longer.Contains(shorter, StringComparison.Ordinal);
    }

    private static bool SharesBrandToken(string a, string b)
    {
        if (a.Length < 4 || b.Length < 4)
            return false;

        var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokensA)
        {
            if (token.Length < 4 || IsGenericNameToken(token))
                continue;
            if (tokensB.Contains(token, StringComparer.Ordinal))
                return true;
        }

        return false;
    }

    private static bool IsGenericNameToken(string token) =>
        token is "кофе" or "кофейня" or "cafe" or "coffee" or "shop" or "bar" or "kitchen";

    private static bool AddressesClose(string a, string b)
    {
        if (a == b)
            return true;

        var tokensA = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokensB = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokensA.Length == 0 || tokensB.Length == 0)
            return false;

        var overlap = tokensA.Intersect(tokensB, StringComparer.Ordinal).Count();
        var denom = Math.Max(tokensA.Length, tokensB.Length);
        return overlap >= 2 && overlap / (double)denom >= 0.5;
    }
}
