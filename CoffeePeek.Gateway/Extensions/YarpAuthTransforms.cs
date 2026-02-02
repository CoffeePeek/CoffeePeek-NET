using System.Security.Claims;
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
        context.AddRequestTransform(async transformContext =>
        {
            var user = transformContext.HttpContext.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                        ClaimsTransformationExtensions.XUserId, userId);

                var userName = user.FindFirst(ClaimTypes.Name)?.Value
                               ?? user.FindFirst("preferred_username")?.Value;
                if (!string.IsNullOrEmpty(userName))
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                        ClaimsTransformationExtensions.XUserName, userName);

                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                        ClaimsTransformationExtensions.XUserEmail, email);

                var role = user.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(role))
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                        ClaimsTransformationExtensions.XUserRole, role);
            }

            await Task.CompletedTask;
        });
    }
}
