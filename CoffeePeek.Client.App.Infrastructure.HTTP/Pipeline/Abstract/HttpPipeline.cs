using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;

public sealed class HttpPipeline(IReadOnlyList<IHttpPipelineStep> steps)
{
    public Task<HttpResult> Send(HttpCommand command, CancellationToken ct = default)
    {
        Func<HttpCommand, Task<HttpResult>> pipeline = _ =>
            Task.FromResult(new HttpResult
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                IsSuccess = false,
                RawBody = string.Empty,
                ErrorMessage = "HTTP pipeline has no send step registered."
            });

        for (var i = steps.Count - 1; i >= 0; i--)
        {
            var step = steps[i];
            var next = pipeline;
            pipeline = cmd => step.Execute(cmd, next, ct);
        }

        return pipeline(command);
    }
}
