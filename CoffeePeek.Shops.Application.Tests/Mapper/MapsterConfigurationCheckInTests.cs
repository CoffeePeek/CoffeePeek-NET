using System;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shops.Application.Mapper;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using FluentAssertions;
using Mapster;

namespace CoffeePeek.Shops.Application.Tests.Mapper;

public class MapsterConfigurationCheckInTests
{
    [Fact]
    public void Adapt_CheckInToCheckInDto_MapsCreatedAtFromCreatedAtUtc()
    {
        var config = MapsterConfiguration.CreateConfig(new MediaPublicUrlOptions());

        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(-1));

        var dto = checkIn.Adapt<CheckInDto>(config);

        dto.CreatedAt.Should().Be(checkIn.CreatedAtUtc);
        dto.CreatedAt.Should().NotBe(default(DateTime));
    }
}
