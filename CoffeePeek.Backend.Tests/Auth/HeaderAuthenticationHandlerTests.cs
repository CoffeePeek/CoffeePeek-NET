using System.Security.Claims;
using System.Text.Encodings.Web;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Auth.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Backend.Tests.Auth;

public class HeaderAuthenticationHandlerTests
{
    [Fact]
    public async Task Authenticate_WithGatewayHeaders_ShouldCreateExpectedClaims()
    {
        var context = new DefaultHttpContext();
        var userId = Guid.NewGuid().ToString();
        context.Request.Headers[GatewayHeaderConsts.XUserId] = userId;
        context.Request.Headers[GatewayHeaderConsts.XUserName] = "coffee-user";
        context.Request.Headers[GatewayHeaderConsts.XUserEmail] = "user@example.com";
        context.Request.Headers[GatewayHeaderConsts.XUserRole] = "Admin, User";

        var result = await CreateHandler().AuthenticateAsync(context);

        result.Succeeded.Should().BeTrue();
        var principal = result.Principal;
        principal.Should().NotBeNull();
        principal!.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(userId);
        principal.FindFirstValue(ClaimTypes.Name).Should().Be("coffee-user");
        principal.FindFirstValue(ClaimTypes.Email).Should().Be("user@example.com");
        principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
            .Should().BeEquivalentTo(["Admin", "User"]);
    }

    [Fact]
    public async Task Authenticate_WithoutUserIdHeader_ShouldNotAuthenticate()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[GatewayHeaderConsts.XUserRole] = RoleConsts.User;

        var result = await CreateHandler().AuthenticateAsync(context);

        result.None.Should().BeTrue();
    }

    private static TestableHeaderAuthenticationHandler CreateHandler()
    {
        return new TestableHeaderAuthenticationHandler(
            new StaticOptionsMonitor<AuthenticationSchemeOptions>(new AuthenticationSchemeOptions()),
            NullLoggerFactory.Instance,
            UrlEncoder.Default);
    }

    private sealed class TestableHeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : HeaderAuthenticationHandler(options, logger, encoder)
    {
        public async Task<AuthenticateResult> AuthenticateAsync(HttpContext context)
        {
            var scheme = new AuthenticationScheme(
                AuthModule.HeaderAuth,
                AuthModule.HeaderAuth,
                typeof(HeaderAuthenticationHandler));

            await InitializeAsync(scheme, context);
            return await HandleAuthenticateAsync();
        }
    }

    private sealed class StaticOptionsMonitor<TOptions>(TOptions value) : IOptionsMonitor<TOptions>
    {
        public TOptions CurrentValue => value;

        public TOptions Get(string? name) => value;

        public IDisposable? OnChange(Action<TOptions, string?> listener) => null;
    }
}
