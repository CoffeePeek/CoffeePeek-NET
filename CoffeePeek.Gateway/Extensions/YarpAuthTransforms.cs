using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CoffeePeek.Gateway.Extensions;

public class ClaimsToHeadersTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var headers = ClaimsTransformationExtensions.ExtractClaimsAsHeaders(transformContext.HttpContext.User);

            foreach (var (key, value) in headers)
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(key, value);
            }

            return ValueTask.CompletedTask;
        });
    }
}
