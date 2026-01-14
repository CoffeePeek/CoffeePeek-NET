using System.Security.Cryptography;
using System.Text;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;

public class SearchCoffeeShopsHandler(
    IGenericRepository<Domain.Entities.CoffeeShopAggregate.CoffeeShop> shopRepository,
    IUserFavoriteRepository favoriteRepository,
    IUserVisitRepository visitRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<SearchCoffeeShopsQuery, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(SearchCoffeeShopsQuery queryRequest,
        CancellationToken cancellationToken)
    {
        var searchHash = CreateSearchHash(queryRequest);
        var cacheKey = CacheKey.Shop.Search(searchHash);

        var cacheResult = await redisService.GetAsync(
            cacheKey,
            async () =>
            {
                var query = shopRepository
                    .QueryAsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(queryRequest.Query))
                {
                    var searchTerm = queryRequest.Query.Trim().ToLower();
                    query = query.Where(s =>
                        s.Name.Contains(searchTerm) ||
                        (s.Location != null &&
                         s.Location.Address.Contains(searchTerm)));
                }

                if (queryRequest.CityId.HasValue)
                {
                    query = query.Where(s => s.Location.CityId == queryRequest.CityId.Value);
                }

                if (queryRequest.Equipments is { Length: > 0 })
                {
                    query = query.Where(s => s.Equipments.Any(se => queryRequest.Equipments.Contains(se.Id)));
                }

                if (queryRequest.Beans is { Length: > 0 })
                {
                    query = query.Where(s =>
                        s.CoffeeBeans.Any(cbs => queryRequest.Beans.Contains(cbs.Id)));
                }

                if (queryRequest.Roasters is { Length: > 0 })
                {
                    query = query.Where(s =>
                        s.Roasters.Any(rs => queryRequest.Roasters.Contains(rs.Id)));
                }

                if (queryRequest.BrewMethods is { Length: > 0 })
                {
                    query = query.Where(s =>
                        s.BrewMethods.Any(sbm =>
                            queryRequest.BrewMethods.Contains(sbm.Id)));
                }

                if (queryRequest.PriceRange.HasValue)
                {
                    query = query.Where(s => s.PriceRange == queryRequest.PriceRange.Value);
                }

                var projectedQuery = query.ProjectToType<ShortShopDto>(mapper.Config);

                var totalCount = await projectedQuery.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)queryRequest.PageSize);

                var shops = await projectedQuery
                    .Skip((queryRequest.PageNumber - 1) * queryRequest.PageSize)
                    .Take(queryRequest.PageSize)
                    .ToArrayAsync(cancellationToken);

                var shopDtos = new List<ShortShopDto>();
                
                var response = new GetCoffeeShopsResponse
                {
                    CoffeeShops = shopDtos,
                    CurrentPage = queryRequest.PageNumber,
                    PageSize = queryRequest.PageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                };

                return Response<GetCoffeeShopsResponse>.Success(response);
            });
        
        if (cacheResult?.Data == null) 
            return Response<GetCoffeeShopsResponse>.Error("Failed to fetch shops.");
        
        if (queryRequest.UserId.HasValue)
        {
            var userId = queryRequest.UserId.Value;
        
            var favoriteIds = await favoriteRepository.GetFavoriteShopIdsAsync(userId, cancellationToken);
            var visitedIds = await visitRepository.GetVisitedShopIdsAsync(userId, cancellationToken);

            var enrichedShops = cacheResult.Data.CoffeeShops.Select(shop => shop with
            {
                IsFavorite = favoriteIds.Contains(shop.Id),
                IsVisited = visitedIds.Contains(shop.Id)
            }).ToList();

            cacheResult.Data.CoffeeShops = enrichedShops;
        }

        return cacheResult;
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