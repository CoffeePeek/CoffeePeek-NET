using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

public static partial class MenuDrinkMatcher
{
    public static CoffeeDrinkDefinition? Match(
        IReadOnlyList<CoffeeDrinkDefinition> catalog,
        string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName) || catalog.Count == 0)
            return null;

        var needle = Normalize(rawName);
        CoffeeDrinkDefinition? best = null;
        var bestLength = 0;

        foreach (var drink in catalog.Where(d => d.IsActive))
        {
            foreach (var alias in EnumerateLabels(drink))
            {
                var hay = Normalize(alias);
                if (hay.Length == 0 || hay.Length < bestLength)
                    continue;

                if (!IsLabelMatch(needle, hay))
                    continue;

                best = drink;
                bestLength = hay.Length;
            }
        }

        return best;
    }

    public static string Normalize(string value)
    {
        var lowered = value.Trim().ToLowerInvariant().Replace('ё', 'е');
        var builder = new StringBuilder(lowered.Length);
        var lastSpace = false;
        foreach (var ch in lowered.Normalize(NormalizationForm.FormD))
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                lastSpace = false;
            }
            else if (!lastSpace)
            {
                builder.Append(' ');
                lastSpace = true;
            }
        }

        return builder.ToString().Trim();
    }

    private static IEnumerable<string> EnumerateLabels(CoffeeDrinkDefinition drink)
    {
        yield return drink.Slug.Replace('_', ' ');
        yield return drink.NameRu;
        yield return drink.NameEn;
        foreach (var alias in drink.AliasList)
            yield return alias;
    }

    private static bool IsLabelMatch(string needle, string label)
    {
        if (needle == label)
            return true;

        return TokenBoundaryRegex(label).IsMatch(needle);
    }

    private static Regex TokenBoundaryRegex(string label)
    {
        var escaped = Regex.Escape(label);
        return new Regex($@"(?:^|\s){escaped}(?:$|\s)", RegexOptions.CultureInvariant);
    }
}
