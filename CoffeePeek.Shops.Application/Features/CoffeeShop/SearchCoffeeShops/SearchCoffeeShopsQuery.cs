using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;

public record SearchCoffeeShopsQuery(
    Guid? UserId = null,
    string? Query = null,
    Guid? CityId = null,
    Guid[]? Roasters = null,
    Guid[]? Equipments = null,
    Guid[]? Beans = null,
    Guid[]? BrewMethods = null,
    PriceRange? PriceRange = null,
    [Range(0, 5)]
    decimal? MinRating = null,
    [Range(1, int.MaxValue)]
    int PageNumber = 1,
    [Range(1, 100)]
    int PageSize = 10);