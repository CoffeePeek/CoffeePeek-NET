using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebCatalogsClient(IHttpCommandExecutor httpCommandExecutor)
    : WebClientBase(httpCommandExecutor), IWebCatalogsClient
{
    public Task<Result<GetCitiesResultDto>> GetCitiesAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/Catalogs/cities");

        return Execute<GetCitiesResultDto>(command, ct);
    }

    public Task<Result<GetBeansResultDto>> GetBeansAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/Catalogs/beans");

        return Execute<GetBeansResultDto>(command, ct);
    }

    public Task<Result<GetRoastersResultDto>> GetRoastersAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/Catalogs/roasters");

        return Execute<GetRoastersResultDto>(command, ct);
    }

    public Task<Result<GetEquipmentResultDto>> GetEquipmentAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/Catalogs/equipments");

        return Execute<GetEquipmentResultDto>(command, ct);
    }

    public Task<Result<GetBrewMethodsResultDto>> GetBrewMethodsAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/Catalogs/brew-methods");

        return Execute<GetBrewMethodsResultDto>(command, ct);
    }
}
