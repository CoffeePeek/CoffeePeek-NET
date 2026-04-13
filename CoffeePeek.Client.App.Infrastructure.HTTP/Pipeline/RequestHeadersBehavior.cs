using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Services.Headers;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;

/// <summary>
/// Applies all <see cref="IHeaderSetter"/> implementations from DI before the request is sent.
/// </summary>
public sealed class RequestHeadersBehavior(IEnumerable<IHeaderSetter> headerSetters) : IHttpPipelineStep
{
    public Task<HttpResult> Execute(
        HttpCommand command,
        Func<HttpCommand, Task<HttpResult>> next,
        CancellationToken ct)
    {
        foreach (var setter in headerSetters)
            setter.SetHeader(command);

        return next(command);
    }
}
