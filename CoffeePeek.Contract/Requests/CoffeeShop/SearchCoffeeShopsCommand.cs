using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record SearchCoffeeShopsCommand(
    string? Query = null,
    Guid? CityId = null,
    Guid[]? Equipments = null,
    Guid[]? Beans = null,
    decimal? MinRating = null,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<Response.Response<GetCoffeeShopsResponse>>;

