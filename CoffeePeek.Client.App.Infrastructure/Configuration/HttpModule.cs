using Autofac;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Services;
using CoffeePeek.Client.App.Infrastructure.HTTP.Services.Headers;
using CoffeePeek.Client.App.Core.Identity;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Infrastructure.Identity;
using CoffeePeek.Client.App.Infrastructure.WebClient;

namespace CoffeePeek.Client.App.Infrastructure.Configuration;

public sealed class HttpModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c =>
        {
            var options = c.Resolve<ApiOptions>();
            var handler = new HttpClientHandler { UseCookies = true };
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(options.BaseAddress.TrimEnd('/') + "/")
            };
        }).SingleInstance();

        builder.Register(c => new TokenRefresher(c.Resolve<HttpClient>(), c.Resolve<AuthClientOptions>()))
            .As<ITokenRefresher>()
            .SingleInstance();

        RegisterHeaderSetters(builder);

        RegisterHttpPipeline(builder);

        builder.RegisterType<HttpCommandExecutor>().As<IHttpCommandExecutor>().SingleInstance();

        builder.RegisterType<WebAuthenticationClient>().As<IWebAuthenticationClient>().SingleInstance();
        builder.RegisterType<WebUsersClient>().As<IWebUsersClient>().SingleInstance();
        builder.RegisterType<WebUserProfileClient>().As<IWebUserProfileClient>().SingleInstance();
        builder.RegisterType<WebUserReviewsClient>().As<IWebUserReviewsClient>().SingleInstance();
        builder.RegisterType<WebCoffeeShopsClient>().As<IWebCoffeeShopsClient>().SingleInstance();
        builder.RegisterType<JwtUserIdentityAccessor>().As<IUserIdentityAccessor>().SingleInstance();
    }

    private static void RegisterHeaderSetters(ContainerBuilder builder)
    {
        var assembly = typeof(AuthorizationHeaderSetter).Assembly;
        builder.RegisterAssemblyTypes(assembly)
            .AssignableTo<IHeaderSetter>()
            .As<IHeaderSetter>()
            .SingleInstance();
    }

    private static void RegisterHttpPipeline(ContainerBuilder builder)
    {
        builder.RegisterType<SendHttpRequestStep>().AsSelf().SingleInstance();
        builder.RegisterType<DeserializeResponseBehavior>().AsSelf().SingleInstance();
        builder.RegisterType<ResilienceBehavior>().AsSelf().SingleInstance();
        builder.RegisterType<RequestHeadersBehavior>().AsSelf().SingleInstance();
        builder.RegisterType<UnauthorizedRefreshBehavior>().AsSelf().SingleInstance();

        builder.Register(c => new HttpPipeline(
        [
            c.Resolve<UnauthorizedRefreshBehavior>(),
            c.Resolve<RequestHeadersBehavior>(),
            c.Resolve<ResilienceBehavior>(),
            c.Resolve<DeserializeResponseBehavior>(),
            c.Resolve<SendHttpRequestStep>()
        ])).AsSelf().SingleInstance();
    }
}
