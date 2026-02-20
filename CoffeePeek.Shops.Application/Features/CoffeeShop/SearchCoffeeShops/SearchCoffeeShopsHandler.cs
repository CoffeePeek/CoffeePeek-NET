using System.Security.Cryptography;
using System.Text;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;

public class SearchCoffeeShopsHandler
{
    public static async Task<Response<GetCoffeeShopsResponse>> Handle(SearchCoffeeShopsQuery queryRequest,
        ICoffeeShopQueries coffeeShopQueries,
        IUserFavoriteRepository favoriteRepository,
        IQueryCheckInRepository visitRepository,
        ICacheService redisService,
        CancellationToken ct)
    {
        var searchHash = CreateSearchHash(queryRequest);
        var cacheKey = CacheKey.Shop.Search(searchHash);

        var cachedResponse = await redisService.GetAsync(cacheKey, async () =>
        {
            var (items, totalCount) = await coffeeShopQueries.Search(queryRequest, ct);
            
            return new GetCoffeeShopsResponse
            {
                CoffeeShops = items,
                TotalItems = totalCount,
                CurrentPage = queryRequest.PageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)queryRequest.PageSize)
            };
        });
        
        if (cachedResponse == null) return Response<GetCoffeeShopsResponse>.Error("Error");

        if (queryRequest.UserId.HasValue)
        {
            var userId = queryRequest.UserId.Value;
            var favoriteIds = await favoriteRepository.GetFavoriteShopIdsAsync(userId, ct);
            var visitedIds = await visitRepository.GetVisitedShopIdsAsync(userId, ct);

            foreach (var shop in cachedResponse.CoffeeShops)
            {
                shop.IsFavorite = favoriteIds.Contains(shop.Id);
                shop.IsVisited = visitedIds.Contains(shop.Id);
            }
        }

        return Response<GetCoffeeShopsResponse>.Success(cachedResponse);
    }

    private static string CreateSearchHash(SearchCoffeeShopsQuery query)
    {
        var keyBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var queryHash = ComputeHash(query.Query.Trim().ToLowerInvariant());
            keyBuilder.Append($"q:{queryHash}:");
        }

        if (query.CityId.HasValue)
        {
            keyBuilder.Append($"city:{query.CityId}:");
        }

        if (query.Equipments is { Length: > 0 })
        {
            var equipmentsHash = ComputeHash(string.Join(",", query.Equipments.OrderBy(x => x)));
            keyBuilder.Append($"eq:{equipmentsHash}:");
        }

        if (query.Beans is { Length: > 0 })
        {
            var beansHash = ComputeHash(string.Join(",", query.Beans.OrderBy(x => x)));
            keyBuilder.Append($"beans:{beansHash}:");
        }

        if (query.Roasters is { Length: > 0 })
        {
            var roastersHash = ComputeHash(string.Join(",", query.Roasters.OrderBy(x => x)));
            keyBuilder.Append($"roasters:{roastersHash}:");
        }

        if (query.BrewMethods is { Length: > 0 })
        {
            var brewMethodsHash = ComputeHash(string.Join(",", query.BrewMethods.OrderBy(x => x)));
            keyBuilder.Append($"brew:{brewMethodsHash}:");
        }

        if (query.PriceRange.HasValue)
        {
            keyBuilder.Append($"price:{(int)query.PriceRange.Value}:");
        }

        if (query.MinRating.HasValue)
        {
            keyBuilder.Append($"rating:{query.MinRating.Value}:");
        }

        keyBuilder.Append($"page:{query.PageNumber}:size:{query.PageSize}");

        return keyBuilder.ToString();
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)[..16].Replace("/", "_").Replace("+", "-");
    }
}