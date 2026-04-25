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
        Guid[]? equipments = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);

    Task<Result<GetCitiesResultDto>> GetCitiesAsync(CancellationToken ct = default);

    Task<Result<GetBeansResultDto>> GetBeansAsync(CancellationToken ct = default);

    Task<Result<GetRoastersResultDto>> GetRoastersAsync(CancellationToken ct = default);

    Task<Result<GetEquipmentResultDto>> GetEquipmentAsync(CancellationToken ct = default);

    Task<Result<GetBrewMethodsResultDto>> GetBrewMethodsAsync(CancellationToken ct = default);

    Task<Result<ShopDetailResultDto>> GetByIdAsync(Guid shopId, CancellationToken ct = default);
}
