using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetShopsInBounds;
using FluentAssertions;
using Moq;

namespace CoffeePeek.Shops.Application.Tests.Features.CoffeeShop.GetShopsInBounds;

public class GetShopsInBoundsHandlerTests
{
    private readonly Mock<ICoffeeShopQueries> _shopQueriesMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsSuccessWithShops()
    {
        var shopId = Guid.NewGuid();
        var shops = new MapShopDto[] { new MapShopDto { Id = shopId } };
        var query = new GetShopsInBoundsQuery(55.0m, 37.0m, 55.5m, 37.5m);

        _shopQueriesMock.Setup(q => q.GetShopsInBounds(query, _ct)).ReturnsAsync(shops);

        var result = await GetShopsInBoundsHandler.Handle(query, _shopQueriesMock.Object, _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Shops.Should().HaveCount(1);
        result.Data.Shops[0].Id.Should().Be(shopId);
    }

    [Fact]
    public async Task Handle_WithNoShopsInBounds_ReturnsSuccessWithEmptyArray()
    {
        var query = new GetShopsInBoundsQuery(0m, 0m, 0.1m, 0.1m);

        _shopQueriesMock.Setup(q => q.GetShopsInBounds(query, _ct)).ReturnsAsync(Array.Empty<MapShopDto>());

        var result = await GetShopsInBoundsHandler.Handle(query, _shopQueriesMock.Object, _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Shops.Should().BeEmpty();
    }
}
