using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public abstract class WebClientBase(IHttpCommandExecutor commandExecutor)
{
    protected async Task<Result> Execute(HttpCommand command, CancellationToken ct = default)
    {
        var response = await commandExecutor.Execute(command, ct);
        return response.IsSuccess
            ? Result.Ok()
            : Result.Fail(response.Message ?? $"HTTP {response.StatusCode}");
    }

    protected async Task<Result<T>> Execute<T>(HttpCommand command, CancellationToken ct = default)
    {
        var response = await commandExecutor.Execute<T>(command, ct);
        if (!response.IsSuccess || response.Data is null)
            return Result.Fail(response.Message ?? $"HTTP {response.StatusCode}");

        return Result.Ok(response.Data);
    }
}
