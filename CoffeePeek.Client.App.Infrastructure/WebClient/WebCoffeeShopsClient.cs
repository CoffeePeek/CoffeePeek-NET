using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebCoffeeShopsClient(IHttpCommandExecutor httpCommandExecutor)
    : WebClientBase(httpCommandExecutor), IWebCoffeeShopsClient
{
    public async Task<Result<SearchShopsResultDto>> SearchAsync(
        string? query = null,
        Guid? cityId = null,
        Guid[]? roasters = null,
        Guid[]? beans = null,
        Guid[]? brewMethods = null,
        Guid[]? equipments = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/CoffeeShops")
            .WithQuery("page", page.ToString())
            .WithQuery("pageSize", pageSize.ToString());

        if (!string.IsNullOrWhiteSpace(query))
            command.WithQuery("q", query);

        if (cityId.HasValue)
            command.WithQuery("cityId", cityId.Value.ToString("D"));

        AppendGuidArray(command, "roasters", roasters);
        AppendGuidArray(command, "beans", beans);
        AppendGuidArray(command, "brewMethods", brewMethods);
        AppendGuidArray(command, "equipments", equipments);

        return await Execute<SearchShopsResultDto>(command, ct);
    }

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

    public Task<Result<ShopDetailResultDto>> GetByIdAsync(Guid shopId, CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint($"api/CoffeeShops/{shopId:D}");

        return Execute<ShopDetailResultDto>(command, ct);
    }

    internal static void AppendGuidArray(HttpCommand command, string key, Guid[]? ids)
    {
        if (ids is null) return;
        for (var i = 0; i < ids.Length; i++)
            command.WithQuery($"{key}[{i}]", ids[i].ToString("D"));
    }
}
