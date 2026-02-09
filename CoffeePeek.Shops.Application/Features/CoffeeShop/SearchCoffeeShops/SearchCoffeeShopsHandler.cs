using System.Security.Cryptography;
using System.Text;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Persistence;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;

public class SearchCoffeeShopsHandler(
    IGenericRepository<Domain.Aggregates.CoffeeShopAggregate.CoffeeShop> shopRepository,
    IReviewRepository reviewRepository,
    IGenericRepository<Domain.Entities.ReviewAggregate.Review> domainReviewRepository,
    ICoffeeShopRepository coffeeShopRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<SearchCoffeeShopsQuery, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(SearchCoffeeShopsQuery queryRequest, 
        CancellationToken cancellationToken)
    {
        var searchHash = CreateSearchHash(queryRequest);
        var cacheKey = CacheKey.Shop.Search(searchHash);

        var response = await redisService.GetAsync(
            cacheKey,
            factory: async () => await GetCoffeeShops(queryRequest, cancellationToken));

        if (response?.Data == null) 
            return Response<GetCoffeeShopsResponse>.Error("Failed to fetch shops.");
        
        if (queryRequest.UserId.HasValue)
        {
            var userId = queryRequest.UserId.Value;
            var shopIds = response.Data.CoffeeShops.Select(s => s.Id).ToList();
        
            var enrichmentMap = await coffeeShopRepository.GetBatchUserShopEnrichmentAsync(userId, shopIds, cancellationToken);

            var enrichedShops = response.Data.CoffeeShops.Select(shop =>
            {
                var e = enrichmentMap.GetValueOrDefault(shop.Id, new UserShopEnrichment(false, false, null));
                return shop with { IsFavorite = e.IsFavorite, IsVisited = e.IsVisited };
            }).ToList();

            response.Data.CoffeeShops = enrichedShops;
        }

        return response;
    }

    private async Task<Response<GetCoffeeShopsResponse>> GetCoffeeShops(SearchCoffeeShopsQuery queryRequest, CancellationToken cancellationToken)
    {
        var query = shopRepository.QueryAsNoTracking().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(queryRequest.Query))
        {
            var term = $"%{queryRequest.Query.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, term) ||
                EF.Functions.ILike(s.Location.Address, term));
        }

        if (queryRequest.CityId.HasValue)
        {
            query = query.Where(s => s.Location.CityId == queryRequest.CityId.Value);
        }

        if (queryRequest.PriceRange.HasValue)
        {
            query = query.Where(s => s.PriceRange == queryRequest.PriceRange.Value);
        }
        
        if (queryRequest.MinRating.HasValue)
        {
            var shopsAboveRating = domainReviewRepository.QueryAsNoTracking()
                .Where(r => !r.IsSoftDelete)
                .GroupBy(r => r.CoffeeShopId)
                .Where(g => g.Average(r => (r.Rating.Place + r.Rating.Coffee + r.Rating.Service) / 3.0m) 
                            >= queryRequest.MinRating.Value)
                .Select(g => g.Key);

            query = query.Where(s => shopsAboveRating.Contains(s.Id));
        }

        if (queryRequest.Equipments is { Length: > 0 })
        {
            query = query.Where(s => s.Equipments.Any(e => queryRequest.Equipments.Contains(e.Id)));
        }

        if (queryRequest.Beans is { Length: > 0 })
        {
            query = query.Where(s => s.CoffeeBeans.Any(e => queryRequest.Beans.Contains(e.Id)));
        }

        if (queryRequest.Roasters is { Length: > 0 })
        {
            query = query.Where(s => s.Roasters.Any(e => queryRequest.Roasters.Contains(e.Id)));
        }

        if (queryRequest.BrewMethods is { Length: > 0 })
        {
            query = query.Where(s => s.BrewMethods.Any(e => queryRequest.BrewMethods.Contains(e.Id)));
        }


        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)queryRequest.PageSize);
        
        var shops  = await query
            .Include(s => s.BrewMethods)
            .Include(s => s.Roasters)
            .Include(s => s.CoffeeBeans)
            .Include(s => s.ShopPhotos)
            .AsSplitQuery()
            .OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Name).ThenBy(x => x.Id)
            .Skip((queryRequest.PageNumber - 1) * queryRequest.PageSize)
            .Take(queryRequest.PageSize)
            .ToListAsync(cancellationToken);
        
        if (shops.Count == 0)
        {
            return Response<GetCoffeeShopsResponse>.Success(new GetCoffeeShopsResponse
            {
                CoffeeShops = [],
                CurrentPage = queryRequest.PageNumber,
                PageSize = queryRequest.PageSize,
                TotalItems = totalCount,
                TotalPages = totalPages
            });
        }

        var reviewStats = await reviewRepository.GetReviewStatsByShopIds(shops.Select(x => x.Id).ToList(), cancellationToken);
        
        var dtos = shops.Select(shop =>
        {
            var reviewStat = reviewStats.GetValueOrDefault(shop.Id, (0, 0));
            var dto = mapper.Map<ShortShopDto>(shop);
            return dto with
            {
                AverageRating = reviewStat.AverageRating,
                ReviewCount = reviewStat.Count
            };
        }).ToList();

        var response = new GetCoffeeShopsResponse
        {
            CoffeeShops = dtos,
            CurrentPage = queryRequest.PageNumber,
            PageSize = queryRequest.PageSize,
            TotalItems = totalCount,
            TotalPages = totalPages
        };

        return Response<GetCoffeeShopsResponse>.Success(response);
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
        
        static string ComputeHash(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash)[..16].Replace("/", "_").Replace("+", "-");
        }
    }
    
}
