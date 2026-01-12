using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Commands.CoffeeShop;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop;

public class GetCoffeeShopsRequestHandler(
    IGenericRepository<Shop> shopRepository,
    IValidationStrategy<GetCoffeeShopsCommand> validationStrategy,
    IMapper mapper,
    IHybridCache hybridCache) 
    : IRequestHandler<GetCoffeeShopsCommand, Response<GetCoffeeShopsResponse>>
{
    public async Task<Response<GetCoffeeShopsResponse>> Handle(GetCoffeeShopsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(command);
        if (!validationResult.IsValid)
        {
            return Response<GetCoffeeShopsResponse>.Error(validationResult.ErrorMessage);
        }

        var cacheKey = CacheKey.Shop.ListByCity(command.CityId, command.PageNumber, command.PageSize);
        
        var result = await hybridCache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var query = shopRepository
                    .QueryAsNoTracking()
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

                return Response<GetCoffeeShopsResponse>.Success(response);
            },
            distributedTtl: cacheKey.DefaultTtl,
            memoryTtl: TimeSpan.FromMinutes(1),
            ct: cancellationToken);

        return result ?? Response<GetCoffeeShopsResponse>.Error("Failed to retrieve coffee shops.");
    }
}