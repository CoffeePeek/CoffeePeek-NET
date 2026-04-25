using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebCoffeeShopsClient
{
    Task<Result<SearchShopsResultDto>> SearchAsync(
        string? query = null,
        Guid? cityId = null,
        Guid[]? roasters = null,
        Guid[]? beans = null,
        Guid[]? brewMethods = null,
        Guid[]? equipment = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);

    Task<Result<ShopDetailResultDto>> GetByIdAsync(Guid shopId, CancellationToken ct = default);
}
