using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;

public interface IHttpPipelineStep
{
    Task<HttpResult> Execute(
        HttpCommand command,
        Func<HttpCommand, Task<HttpResult>> next,
        CancellationToken ct);
}
