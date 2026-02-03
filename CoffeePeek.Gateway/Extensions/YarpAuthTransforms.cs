using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CoffeePeek.Gateway.Extensions;

public class ClaimsToHeadersTransformProvider : ITransformProvider
{
    /// <summary>
    /// Выполняет проверку конфигурации маршрута для этого поставщика преобразований запросов.
    /// </summary>
    /// <param name="context">Контекст валидации маршрута, содержащий данные маршрута и коллекцию сообщений валидации.</param>
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    /// <summary>
    /// Выполняет проверку конфигурации кластера для этого провайдера преобразований.
    /// В настоящий момент проверка не выполняется.
    /// </summary>
    /// <param name="context">Контекст валидации, содержащий данные и ошибки валидации для кластера.</param>
    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    /// <summary>
    /// Регистрирует трансформацию исходящих запросов, которая при аутентифицированном пользователе добавляет значения определённых claim в заголовки проксируемого запроса.
    /// </summary>
    /// <param name="context">Контекст построителя трансформаций, используемый для добавления трансформации запроса. Добавляемые заголовки: NameIdentifier -> XUserId, Name или preferred_username -> XUserName, Email -> XUserEmail, Role -> XUserRole.</param>
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