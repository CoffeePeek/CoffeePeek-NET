using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

namespace CoffeePeek.Moderation.Domain.Import;

public static class CoffeeMapClassifier
{
    public static (ImportCollectorBucket Bucket, IReadOnlyList<string> Signals) Classify(
        CoffeeMapCandidateSnapshot snapshot)
    {
        var signals = new List<string> { "coffeemap" };

        if (snapshot.IsSpecialty)
            signals.Add("coffeemap:specialty");
        if (snapshot.Recommended)
            signals.Add("coffeemap:recommended");
        if (snapshot.GoogleRating is { } rating)
            signals.Add($"coffeemap:google-rating={rating.ToString("0.0")}");
        foreach (var amenity in snapshot.AmenitySignals)
            signals.Add(amenity);

        var name = snapshot.Name ?? "";
        ImportCollectorBucket bucket;
        if (snapshot.IsSpecialty)
            bucket = ImportCollectorBucket.LikelySpecialty;
        else if (snapshot.Recommended)
            bucket = ImportCollectorBucket.Priority;
        else if (snapshot.GoogleRating >= 4.4 && (snapshot.GoogleRatingsCount ?? 0) >= 20)
        {
            signals.Add("coffeemap:well-rated");
            bucket = ImportCollectorBucket.Priority;
        }
        else if (name.Contains("кофе", StringComparison.OrdinalIgnoreCase) ||
                 name.Contains("coffee", StringComparison.OrdinalIgnoreCase))
        {
            signals.Add("name:coffee");
            bucket = ImportCollectorBucket.Priority;
        }
        else
            bucket = ImportCollectorBucket.Review;

        return (bucket, signals);
    }
}
