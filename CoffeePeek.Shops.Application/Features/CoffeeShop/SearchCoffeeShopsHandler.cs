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
    : IRequestHandler<SearchCoffeeShopsCommand, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(SearchCoffeeShopsCommand command, CancellationToken cancellationToken)
    {
        var searchHash = CreateSearchHash(command);
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

        if (!string.IsNullOrWhiteSpace(command.Query))
        {
            var searchTerm = command.Query.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchTerm) ||
                (s.Location != null && s.Location.Address.ToLower().Contains(searchTerm)));
        }

        if (command.CityId.HasValue)
        {
            query = query.Where(s => s.CityId == command.CityId.Value);
        }
        
        if (command.Equipments is { Length: > 0 })
        {
            query = query.Where(s => s.ShopEquipments.Any(se => command.Equipments.AsEnumerable().Contains(se.EquipmentId)));
        }

        if (command.Beans is { Length: > 0 })
        {
            query = query.Where(s => s.CoffeeBeanShops.Any(cbs => command.Beans.AsEnumerable().Contains(cbs.CoffeeBeanId)));
        }

        if (command.MinRating.HasValue)
        {
            var minRating = command.MinRating.Value;
            
            query = query.Where(s => s.Reviews.Any() && 
                s.Reviews.Sum(r => r.RatingCoffee + r.RatingPlace + r.RatingService) >= 
                minRating * s.Reviews.Count * 3m);
        }

        var projectedQuery = query.ProjectToType<ShortShopDto>(mapper.Config);

        var totalCount = await projectedQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)command.PageSize);

        var shopDtos = await projectedQuery
            .Skip((command.PageNumber - 1) * command.PageSize)
            .Take(command.PageSize)
            .ToArrayAsync(cancellationToken);

                var response = new GetCoffeeShopsResponse
                {
                    CoffeeShops = shopDtos,
                    CurrentPage = command.PageNumber,
                    PageSize = command.PageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                };

                return Response<GetCoffeeShopsResponse>.Success(response);
            },
            TimeSpan.FromMinutes(5));
        
        return result ?? Response<GetCoffeeShopsResponse>.Error("Failed to search coffee shops.");
    }

    private static string CreateSearchHash(SearchCoffeeShopsCommand command)
    {
        var keyBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(command.Query))
        {
            var queryHash = ComputeHash(command.Query.Trim().ToLowerInvariant());
            keyBuilder.Append($"q:{queryHash}:");
        }
        
        if (command.CityId.HasValue)
        {
            keyBuilder.Append($"city:{command.CityId}:");
        }
        
        if (command.Equipments is { Length: > 0 })
        {
            var equipmentsHash = ComputeHash(string.Join(",", command.Equipments.OrderBy(x => x)));
            keyBuilder.Append($"eq:{equipmentsHash}:");
        }
        
        if (command.Beans is { Length: > 0 })
        {
            var beansHash = ComputeHash(string.Join(",", command.Beans.OrderBy(x => x)));
            keyBuilder.Append($"beans:{beansHash}:");
        }
        
        if (command.MinRating.HasValue)
        {
            keyBuilder.Append($"rating:{command.MinRating.Value}:");
        }
        
        keyBuilder.Append($"page:{command.PageNumber}:size:{command.PageSize}");
        
        return keyBuilder.ToString();
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)[..16].Replace("/", "_").Replace("+", "-");
    }
}

