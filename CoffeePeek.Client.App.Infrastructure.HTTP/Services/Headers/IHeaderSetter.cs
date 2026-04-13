using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Services.Headers;

/// <summary>
/// Contributes HTTP headers before send. Register implementations as <see cref="IHeaderSetter"/> in Autofac
/// (see <see cref="Configuration.HttpModule"/> assembly scan).
/// </summary>
public interface IHeaderSetter
{
    /// <summary>Mutates <paramref name="command"/> headers when applicable.</summary>
    void SetHeader(HttpCommand command);
}