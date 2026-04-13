using System.Net.Http;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

/// <summary>Single-use command: do not share instances across concurrent sends.</summary>
public class HttpCommand
{
    public string Endpoint { get; set; } = string.Empty;

    public HttpMethod Method { get; set; } = HttpMethod.Get;

    public BaseRequest? Body { get; set; }
    
    public bool IsAuthorize { get; set; }

    /// <summary>
    /// When true, <see cref="UnauthorizedRefreshBehavior"/> will not run refresh+retry (avoids loops for internal calls).
    /// </summary>
    public bool SkipUnauthorizedRefreshRetry { get; set; }

    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Query { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// When set before send, <see cref="Pipeline.DeserializeResponseBehavior"/> deserializes <c>data</c> as this type
    /// inside <see cref="Serialization.ApiEnvelopeDto{T}"/>. Cleared by <see cref="Abstract.HttpCommandExecutor"/>.
    /// </summary>
    public Type? ResponsePayloadType { get; set; }
}
