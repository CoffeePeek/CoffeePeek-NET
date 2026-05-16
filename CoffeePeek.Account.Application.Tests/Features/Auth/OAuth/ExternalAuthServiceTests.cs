using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.Auth.OAuthLogin;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.Auth.OAuth;

public class ExternalAuthServiceTests
{
    private readonly Mock<IQueryUserRepository> _queryRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private ExternalAuthService CreateSut() => new(_queryRepoMock.Object, _roleRepoMock.Object);

    public ExternalAuthServiceTests()
    {
        _roleRepoMock.Setup(r => r.GetRoleAsync(RoleConsts.User)).ReturnsAsync(Role.Create(RoleConsts.User));
    }

    [Fact]
    public async Task GetOrCreate_WhenFoundByProvider_ReturnsExistingUser()
    {
        var existingUser = DomainUser.CreateExternal("user@google.com", "google", "google_id_123");
        _queryRepoMock.Setup(r => r.GetByProvider("google", "google_id_123", _ct)).ReturnsAsync(existingUser);
        var result = await CreateSut().GetOrCreate("user@google.com", "google", "google_id_123", _ct);
        result.Should().Be(existingUser);
        _queryRepoMock.Verify(r => r.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _queryRepoMock.Verify(r => r.Add(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreate_WhenFoundByEmailNotProvider_ThrowsUnauthorizedException()
    {
        var existingUser = DomainUser.Register("user@example.com", "testuser", "hash", Role.Create("User"));
        _queryRepoMock.Setup(r => r.GetByProvider("google", "google_id", _ct)).ReturnsAsync((DomainUser?)null);
        _queryRepoMock.Setup(r => r.GetByEmail("user@example.com", _ct)).ReturnsAsync(existingUser);
        Func<Task> act = () => CreateSut().GetOrCreate("user@example.com", "google", "google_id", _ct);
        await act.Should().ThrowAsync<UnauthorizedException>();
        _queryRepoMock.Verify(r => r.Add(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreate_WhenNotFound_CreatesNewUser()
    {
        _queryRepoMock.Setup(r => r.GetByProvider(It.IsAny<string>(), It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        _queryRepoMock.Setup(r => r.GetByEmail(It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        var result = await CreateSut().GetOrCreate("newuser@google.com", "google", "new_id", _ct);
        result.Credentials.Email.Value.Should().Be("newuser@google.com");
        result.Credentials.OAuthProvider.Should().Be("google");
        _queryRepoMock.Verify(r => r.Add(result, _ct), Times.Once);
    }

    [Fact]
    public async Task GetOrCreate_WhenNotFound_AssignsUserRole()
    {
        _queryRepoMock.Setup(r => r.GetByProvider(It.IsAny<string>(), It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        _queryRepoMock.Setup(r => r.GetByEmail(It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        var result = await CreateSut().GetOrCreate("newuser@google.com", "google", "id", _ct);
        result.Roles.Should().ContainSingle(r => r.Name == RoleConsts.User);
    }

    [Fact]
    public async Task GetOrCreate_WhenNotFoundAndRoleMissing_ThrowsApplicationException()
    {
        _queryRepoMock.Setup(r => r.GetByProvider(It.IsAny<string>(), It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        _queryRepoMock.Setup(r => r.GetByEmail(It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        _roleRepoMock.Setup(r => r.GetRoleAsync(RoleConsts.User)).ReturnsAsync((Role?)null);
        Func<Task> act = () => CreateSut().GetOrCreate("user@google.com", "google", "id", _ct);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task GetOrCreate_WhenNotFound_NewUserHasEmailConfirmed()
    {
        _queryRepoMock.Setup(r => r.GetByProvider(It.IsAny<string>(), It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        _queryRepoMock.Setup(r => r.GetByEmail(It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        var result = await CreateSut().GetOrCreate("user@google.com", "google", "id", _ct);
        result.Credentials.EmailConfirmed.Should().BeTrue();
    }
}
