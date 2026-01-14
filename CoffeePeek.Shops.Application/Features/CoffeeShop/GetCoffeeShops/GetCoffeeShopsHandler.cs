using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShops;

public class GetCoffeeShopsHandler(
    IGenericRepository<Domain.Entities.CoffeeShopAggregate.CoffeeShop> shopRepository,
    IValidationStrategy<GetCoffeeShopsQuery> validationStrategy,
    IUserFavoriteService favoriteService,
    IUserVisitService visitService,
    IMapper mapper,
    IRedisService redisService) 
    : IRequestHandler<GetCoffeeShopsQuery, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(GetCoffeeShopsQuery queryRequest, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(queryRequest);
        if (!validationResult.IsValid)
        {
            return Response<GetCoffeeShopsResponse>.Error(validationResult.ErrorMessage);
        }

        var cacheKey = CacheKey.Shop.ListByCity(queryRequest.CityId, queryRequest.PageNumber, queryRequest.PageSize);
        
        var result = await redisService.GetAsync(
            cacheKey,
            async () =>
            {
                var query = shopRepository
                    .QueryAsNoTracking()
                    .Include(s => s.Reviews)
                    .Where(s => s.Location.CityId == queryRequest.CityId)
                    .ProjectToType<ShortShopDto>(mapper.Config);

                var totalCount = await query.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)queryRequest.PageSize); 

                var shops = await query
                    .Skip((queryRequest.PageNumber - 1) * queryRequest.PageSize)
                    .Take(queryRequest.PageSize)
                    .ToArrayAsync(cancellationToken);

                var shopDtos = new List<ShortShopDto>();
    
                foreach (var shop in shops)
                {
                    var dto = mapper.Map<ShortShopDto>(shop);
        
                    if (queryRequest.UserId.HasValue)
                    {
                        dto = dto with 
                        { 
                            IsFavorite = await favoriteService.IsFavoriteAsync(queryRequest.UserId.Value, shop.Id, cancellationToken),
                            IsVisited = await visitService.HasVisitedAsync(queryRequest.UserId.Value, shop.Id, cancellationToken)
                        };
                    }
        
                    shopDtos.Add(dto);
                }

                var response = new GetCoffeeShopsResponse
                {
                    CoffeeShops = shopDtos,
                    CurrentPage = queryRequest.PageNumber,
                    PageSize = queryRequest.PageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                };

                return Response<GetCoffeeShopsResponse>.Success(response);
            },
            cacheKey.DefaultTtl);   

        return result ?? Response<GetCoffeeShopsResponse>.Error("Failed to retrieve coffee shops.");
    }
}