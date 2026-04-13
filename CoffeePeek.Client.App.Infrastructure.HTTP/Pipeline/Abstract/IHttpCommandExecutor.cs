using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;

public interface IHttpCommandExecutor
{
    Task<ApiResponse> Execute(HttpCommand command, CancellationToken ct);

    Task<ApiResponse<T>> Execute<T>(HttpCommand command, CancellationToken ct);
}
