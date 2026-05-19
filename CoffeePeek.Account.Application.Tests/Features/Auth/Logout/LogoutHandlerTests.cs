using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.Auth.Logout;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.Auth.Logout;

public class LogoutHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static DomainUser CreateUserWithSession(string token = "refresh_token")
    {
        var role = Role.Create("User");
        var user = DomainUser.Register("user@example.com", "testuser", "hash", role);
        user.AddSession(token, TimeSpan.FromDays(7), "Chrome", "127.0.0.1");
        return user;
    }

    [Fact]
    public async Task Handle_WithValidUser_RevokesToken()
    {
        var user = CreateUserWithSession("my_token");
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        await LogoutHandler.Handle(new LogoutCommand(user.Id, "my_token"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        user.RefreshTokens.Single(t => t.Token == "my_token").IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithValidUser_CallsSaveChanges()
    {
        var user = CreateUserWithSession();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        await LogoutHandler.Handle(new LogoutCommand(user.Id, "refresh_token"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetById(It.IsAny<Guid>(), _ct)).ReturnsAsync((DomainUser?)null);
        Func<Task> act = () => LogoutHandler.Handle(new LogoutCommand(Guid.NewGuid(), "token"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_WithNonExistentToken_DoesNotThrow()
    {
        var user = CreateUserWithSession("real_token");
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        Func<Task> act = () => LogoutHandler.Handle(new LogoutCommand(user.Id, "nonexistent"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        await act.Should().NotThrowAsync();
        user.RefreshTokens.Single(t => t.Token == "real_token").IsActive.Should().BeTrue();
    }
}
