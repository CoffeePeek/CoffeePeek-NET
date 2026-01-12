using System.Security.Cryptography;
using System.Text;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop;

public class SearchCoffeeShopsHandler(
    IGenericRepository<Shop> shopRepository,
    IMapper mapper,
    IRedisService redisService)
    : IRequestHandler<SearchCoffeeShopsQuery, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(SearchCoffeeShopsQuery queryRequest, CancellationToken cancellationToken)
    {
        var searchHash = CreateSearchHash(queryRequest);
        var cacheKey = CacheKey.Shop.Search(searchHash);
        
        var result = await redisService.GetAsync(
            cacheKey,
            async () =>
            {

        var query = shopRepository.QueryAsNoTracking()
            .Include(s => s.Location)
            .Include(s => s.Reviews)
            .Include(s => s.ShopPhotos)
            .Include(s => s.ShopEquipments)
                .ThenInclude(se => se.Equipment)
            .Include(s => s.CoffeeBeanShops)
                .ThenInclude(cbs => cbs.CoffeeBean)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryRequest.Query))
        {
            var searchTerm = queryRequest.Query.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchTerm) ||
                (s.Location != null && s.Location.Address.ToLower().Contains(searchTerm)));
        }

        if (queryRequest.CityId.HasValue)
        {
            query = query.Where(s => s.CityId == queryRequest.CityId.Value);
        }
        
        if (queryRequest.Equipments is { Length: > 0 })
        {
            query = query.Where(s => s.ShopEquipments.Any(se => queryRequest.Equipments.AsEnumerable().Contains(se.EquipmentId)));
        }

        if (queryRequest.Beans is { Length: > 0 })
        {
            query = query.Where(s => s.CoffeeBeanShops.Any(cbs => queryRequest.Beans.AsEnumerable().Contains(cbs.CoffeeBeanId)));
        }

        if (queryRequest.MinRating.HasValue)
        {
            var minRating = queryRequest.MinRating.Value;
            
            query = query.Where(s => s.Reviews.Any() && 
                s.Reviews.Sum(r => r.RatingCoffee + r.RatingPlace + r.RatingService) >= 
                minRating * s.Reviews.Count * 3m);
        }

        var projectedQuery = query.ProjectToType<ShortShopDto>(mapper.Config);

        var totalCount = await projectedQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)queryRequest.PageSize);

        var shopDtos = await projectedQuery
            .Skip((queryRequest.PageNumber - 1) * queryRequest.PageSize)
            .Take(queryRequest.PageSize)
            .ToArrayAsync(cancellationToken);

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
        
        return result ?? Response<GetCoffeeShopsResponse>.Error("Failed to search coffee shops.");
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

