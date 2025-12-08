using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using CoffeePeek.ShopsService.Abstractions.ValidationStrategy;
using CoffeePeek.ShopsService.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop;

public class GetCoffeeShopsRequestHandler(
    IGenericRepository<Shop> shopRepository,
    IValidationStrategy<GetCoffeeShopsCommand> validationStrategy,
    IMapper mapper,
    IRedisService redisService) 
    : IRequestHandler<GetCoffeeShopsCommand, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(GetCoffeeShopsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(command);
        if (!validationResult.IsValid)
        {
            return Response<GetCoffeeShopsResponse>.Error(validationResult.ErrorMessage);
        }

        var cacheKey = CacheKey.Shop.ByCity(command.CityId, command.PageNumber, command.PageSize);
        var cached = await redisService.GetAsync<Response<GetCoffeeShopsResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var query = shopRepository.QueryAsNoTracking()
            .Where(s => s.CityId == command.CityId)
            .ProjectToType<ShortShopDto>(mapper.Config);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)command.PageSize); 

        var shopDtos = await query
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

        var result = Response<GetCoffeeShopsResponse>.Success(response);
        await redisService.SetAsync(cacheKey, result);
        return result;
    }
}