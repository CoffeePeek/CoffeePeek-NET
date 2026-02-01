using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;

public record GetUserCheckInsResponse(
    CheckInDto[] CheckIns,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);