using System.Text;
using CoffeePeek.Client.App.Core.Security;
using CoffeePeek.Client.App.Infrastructure.Cache;
using CoffeePeek.Client.App.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace CoffeePeek.Client.App.Tests.Identity;

public class JwtUserRoleAccessorTests
{
    [Fact]
    public void IsInRole_True_WhenTokenContainsRoleString()
    {
        var session = new ClientSession();
        session.SetAccessToken(MakeJwt("""{"sub":"7c9e6679-7425-40de-944b-e07fc1f90ae7","role":"Moderator"}"""));
        var sut = new JwtUserRoleAccessor(session);

        sut.IsInRole(WellKnownAppRoles.Moderator).Should().BeTrue();
    }

    [Fact]
    public void IsInRole_True_WhenTokenContainsRoleString_IgnoreCase()
    {
        var session = new ClientSession();
        session.SetAccessToken(MakeJwt("""{"sub":"7c9e6679-7425-40de-944b-e07fc1f90ae7","role":"moderator"}"""));
        var sut = new JwtUserRoleAccessor(session);

        sut.IsInRole(WellKnownAppRoles.Moderator).Should().BeTrue();
    }

    [Fact]
    public void IsInRole_False_WhenNoToken()
    {
        var session = new ClientSession();
        var sut = new JwtUserRoleAccessor(session);

        sut.IsInRole(WellKnownAppRoles.Moderator).Should().BeFalse();
    }

    private static string MakeJwt(string jsonPayload)
    {
        return $"{B64("""{"alg":"none","typ":"JWT"}""")}.{B64(jsonPayload)}.";
    }

    private static string B64(string s)
    {
        return Convert
            .ToBase64String(Encoding.UTF8.GetBytes(s))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
