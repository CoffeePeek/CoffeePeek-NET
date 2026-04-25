using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebCatalogsClient
{
    Task<Result<GetCitiesResultDto>> GetCitiesAsync(CancellationToken ct = default);

    Task<Result<GetBeansResultDto>> GetBeansAsync(CancellationToken ct = default);

    Task<Result<GetRoastersResultDto>> GetRoastersAsync(CancellationToken ct = default);

    Task<Result<GetEquipmentResultDto>> GetEquipmentAsync(CancellationToken ct = default);

    Task<Result<GetBrewMethodsResultDto>> GetBrewMethodsAsync(CancellationToken ct = default);
}
