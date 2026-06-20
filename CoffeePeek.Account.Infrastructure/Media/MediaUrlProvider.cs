using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Infrastructure.Media;

public class MediaUrlProvider(IOptions<MinIOOptions> options) : IMediaUrlProvider
{
    public string? BuildAvatarUrl(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return null;

        var endpoint = options.Value.Endpoint.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(endpoint))
            return null;

        return $"{endpoint}/coffee.avatars/{storageKey}";
    }
}
