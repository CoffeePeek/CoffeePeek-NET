using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Services.Headers;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;

/// <summary>
/// Applies all <see cref="IHeaderSetter"/> implementations from DI before the request is sent.
/// </summary>
/// <remarks>
/// IMPORTANT: Header setters modify <see cref="HttpCommand.Headers"/> per-request, NOT <see cref="HttpClient.DefaultRequestHeaders"/>.
/// HttpClient.DefaultRequestHeaders must NEVER be mutated to ensure presigned upload URLs are not polluted with auth headers.
/// </remarks>
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
