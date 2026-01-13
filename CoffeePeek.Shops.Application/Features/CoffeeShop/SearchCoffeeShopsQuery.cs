using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop;

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
    int PageSize = 10)
    : IRequest<Response<GetCoffeeShopsResponse>>;