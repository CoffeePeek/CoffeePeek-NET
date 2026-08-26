using CoffeePeek.Contract.Dtos.Menu;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Menu;
using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetMenuDrinks;

public record GetMenuDrinksQuery;

public record GetMenuDrinksResponse(CoffeeDrinkDefinitionDto[] Drinks);

public static class GetMenuDrinksHandler
{
    public static async Task<Response<GetMenuDrinksResponse>> Handle(
        GetMenuDrinksQuery _,
        IQueryCoffeeDrinkRepository repository,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var cacheKey = CacheKey.Shop.MenuDrinks();
        var drinks = await cacheService.GetAsync(cacheKey, async token =>
        {
            var active = await repository.GetActiveAsync(token);
            return active.Select(ShopMenuDtoFactory.ToDto).ToArray();
        }, cancellationToken: ct);

        return drinks is null
            ? Response<GetMenuDrinksResponse>.Error("Failed to retrieve drink catalog.")
            : Response<GetMenuDrinksResponse>.Success(new GetMenuDrinksResponse(drinks));
    }
}
