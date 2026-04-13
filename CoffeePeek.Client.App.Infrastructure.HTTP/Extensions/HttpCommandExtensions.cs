using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;

public static class HttpCommandExtensions
{
    extension(HttpCommand command)
    {
        public HttpCommand WithEndpoint(string endpoint)
        {
            command.Endpoint = endpoint;
            return command;
        }

        public HttpCommand WithMethod(HttpMethod method)
        {
            command.Method = method;
            return command;
        }

        public HttpCommand Authorize()
        {
            command.IsAuthorize = true;
            return command;
        }

        public HttpCommand SkipRefreshOn401Retry()
        {
            command.SkipUnauthorizedRefreshRetry = true;
            return command;
        }

        public HttpCommand WithBody(BaseRequest? body)
        {
            command.Body = body;
            return command;
        }

        public HttpCommand WithHeader(string name, string value)
        {
            command.Headers[name] = value;
            return command;
        }

        public HttpCommand WithQuery(string name, string value)
        {
            command.Query[name] = value;
            return command;
        }
    }
}
