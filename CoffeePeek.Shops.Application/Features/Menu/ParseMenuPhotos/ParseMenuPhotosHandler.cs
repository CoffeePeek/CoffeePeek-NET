using CoffeePeek.Contract.Dtos.Menu;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Abstractions;
using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shops.Application.Features.Menu.ParseMenuPhotos;

public record ParseMenuPhotoInput(string StorageKey, string ContentType, string? PublicUrl);

public record ParseMenuPhotosCommand(IReadOnlyList<ParseMenuPhotoInput> Photos);

public record ParsedMenuItemDto(
    string Slug,
    decimal? Price,
    int? VolumeMl,
    string RawName);

public record ParseMenuPhotosResponse(
    bool Success,
    string? Error,
    PriceRange? SuggestedPriceRange,
    IReadOnlyList<ParsedMenuItemDto> Items,
    IReadOnlyList<UnmatchedMenuItemDto> Unmatched);

public static class ParseMenuPhotosHandler
{
    public static async Task<Response<ParseMenuPhotosResponse>> Handle(
        ParseMenuPhotosCommand command,
        IMenuVisionParser parser,
        IQueryCoffeeDrinkRepository drinkRepository,
        IMenuPhotoDownloader photoDownloader,
        IOptions<MenuPriceRangeOptions> priceOptions,
        CancellationToken ct)
    {
        var photos = command.Photos
            .Where(p => !string.IsNullOrWhiteSpace(p.PublicUrl))
            .Take(BusinessConstants.MaxMenuPhotosPerParse)
            .Select(p => new MenuPhotoDownloadRequest(p.PublicUrl!, p.ContentType))
            .ToArray();

        if (photos.Length == 0)
            return Response<ParseMenuPhotosResponse>.Success(Failed("No menu photo URLs to parse."));

        var downloaded = await photoDownloader.DownloadAsync(photos, ct);

        if (downloaded.Count == 0)
            return Response<ParseMenuPhotosResponse>.Success(Failed("Could not download menu photos."));

        var vision = await parser.ParseAsync(downloaded, ct);
        if (!vision.Success)
            return Response<ParseMenuPhotosResponse>.Success(Failed(vision.Error ?? "Parse failed."));

        var catalog = await drinkRepository.GetActiveAsync(ct);
        var matched = new Dictionary<string, ParsedMenuItemDto>(StringComparer.OrdinalIgnoreCase);
        var unmatched = new List<UnmatchedMenuItemDto>();

        foreach (var line in vision.Drinks)
        {
            var drink = MenuDrinkMatcher.Match(catalog, line.RawName);
            if (drink is null)
            {
                unmatched.Add(new UnmatchedMenuItemDto(line.RawName, line.Price, line.Confidence));
                continue;
            }

            if (matched.TryGetValue(drink.Slug, out var existing))
            {
                var minPrice = MinPrice(existing.Price, line.Price);
                matched[drink.Slug] = existing with { Price = minPrice };
                continue;
            }

            matched[drink.Slug] = new ParsedMenuItemDto(drink.Slug, line.Price, line.VolumeMl, line.RawName);
        }

        var items = matched.Values.ToArray();
        var settings = priceOptions.Value;
        var suggested = MenuPriceRangeCalculator.FromPrices(
            items.Where(i => i.Price.HasValue).Select(i => i.Price!.Value),
            settings.CheapBelow,
            settings.ExpensiveAbove);

        return Response<ParseMenuPhotosResponse>.Success(new ParseMenuPhotosResponse(
            true,
            null,
            suggested is null ? null : (PriceRange)(int)suggested.Value,
            items,
            unmatched));
    }

    private static ParseMenuPhotosResponse Failed(string error) =>
        new(false, error, null, [], []);

    private static decimal? MinPrice(decimal? left, decimal? right)
    {
        if (left is null) return right;
        if (right is null) return left;
        return Math.Min(left.Value, right.Value);
    }
}
