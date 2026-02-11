using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.CheckIn;

public interface ICheckInQueries
{
    Task<CheckInDto[]> GetByUserId(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default);
}