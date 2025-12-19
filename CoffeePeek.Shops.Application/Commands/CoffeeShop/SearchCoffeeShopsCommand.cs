using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record SearchCoffeeShopsCommand(
    string? Query = null,
    Guid? CityId = null,
    Guid[]? Equipments = null,
    Guid[]? Beans = null,
    [Range(0, 5)]
    decimal? MinRating = null,
    [Range(1, int.MaxValue)]
    int PageNumber = 1,
    [Range(1, 100)]

    int PageSize = 10)
    : IRequest<Response<GetCoffeeShopsResponse>>;

