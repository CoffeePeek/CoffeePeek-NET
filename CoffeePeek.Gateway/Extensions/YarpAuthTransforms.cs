using CoffeePeek.Shared.Auth.Constants;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CoffeePeek.Gateway.Extensions;

/// <summary>
/// YARP transform provider that strips incoming <see cref="GatewayHeaderConsts"/> headers
/// (to prevent client impersonation) and then re-adds them from the validated JWT claims.
/// Applied globally to every proxied route.
/// </summary>
public class ClaimsToHeadersTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }

    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            // Security: strip any X-User-* headers sent by the client to prevent impersonation.
            // Downstream services must only trust these headers when they originate from the gateway.
            var requestHeaders = transformContext.ProxyRequest.Headers;
            requestHeaders.Remove(GatewayHeaderConsts.XUserId);
            requestHeaders.Remove(GatewayHeaderConsts.XUserName);
            requestHeaders.Remove(GatewayHeaderConsts.XUserRole);
            requestHeaders.Remove(GatewayHeaderConsts.XUserEmail);

            // Now add the verified, gateway-issued claims headers.
            var claimsHeaders = ClaimsTransformationExtensions.ExtractClaimsAsHeaders(transformContext.HttpContext.User);
            foreach (var (key, value) in claimsHeaders)
            {
                requestHeaders.TryAddWithoutValidation(key, value);
            }

            return ValueTask.CompletedTask;
        });
    }
}
