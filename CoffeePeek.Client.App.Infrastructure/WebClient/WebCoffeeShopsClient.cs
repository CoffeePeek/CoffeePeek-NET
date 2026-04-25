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

        return await Execute<SearchShopsResultDto>(command, ct);
    }

    public Task<Result<GetCitiesResultDto>> GetCitiesAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithEndpoint("api/Catalogs/cities");

        return Execute<GetCitiesResultDto>(command, ct);
    }
}
