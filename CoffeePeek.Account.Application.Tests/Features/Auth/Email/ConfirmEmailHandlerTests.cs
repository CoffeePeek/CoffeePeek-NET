using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.Auth.Email;

public class ConfirmEmailHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static DomainUser CreateUnconfirmedUser(string token = "valid_token")
    {
        var role = Role.Create("User");
        var user = DomainUser.Register("user@example.com", "testuser", "hash", role);
        typeof(UserCredential).GetProperty(nameof(UserCredential.EmailConfirmationToken))!
            .SetValue(user.Credentials, token);
        typeof(UserCredential).GetProperty(nameof(UserCredential.EmailConfirmationExpiresAt))!
            .SetValue(user.Credentials, DateTime.UtcNow.AddMinutes(10));
        return user;
    }

    [Fact]
    public async Task Handle_WithValidToken_ReturnsSuccess()
    {
        var user = CreateUnconfirmedUser("valid_confirm_token");
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken("valid_confirm_token", _ct)).ReturnsAsync(user);
        var result = await ConfirmEmailHandler.Handle(new ConfirmEmailCommand("valid_confirm_token"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidToken_SetsEmailConfirmed()
    {
        var user = CreateUnconfirmedUser("tok");
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken("tok", _ct)).ReturnsAsync(user);
        await ConfirmEmailHandler.Handle(new ConfirmEmailCommand("tok"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        user.Credentials.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidToken_CallsSaveChanges()
    {
        var user = CreateUnconfirmedUser("tok");
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken("tok", _ct)).ReturnsAsync(user);
        await ConfirmEmailHandler.Handle(new ConfirmEmailCommand("tok"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ThrowsNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken(It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);
        Func<Task> act = () => ConfirmEmailHandler.Handle(new ConfirmEmailCommand("bad"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_WithTokenNotFoundInDb_ThrowsNotFoundException()
    {
        // When a wrong/random token is passed, the repo returns null → NotFoundException
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken("wrong_token", _ct)).ReturnsAsync((DomainUser?)null);
        Func<Task> act = () => ConfirmEmailHandler.Handle(new ConfirmEmailCommand("wrong_token"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ThrowsDomainException()
    {
        var user = CreateUnconfirmedUser("expired");
        typeof(UserCredential).GetProperty(nameof(UserCredential.EmailConfirmationExpiresAt))!
            .SetValue(user.Credentials, DateTime.UtcNow.AddMinutes(-1));
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken("expired", _ct)).ReturnsAsync(user);
        Func<Task> act = () => ConfirmEmailHandler.Handle(new ConfirmEmailCommand("expired"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        await act.Should().ThrowAsync<DomainException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmed_ThrowsDomainException()
    {
        var user = CreateUnconfirmedUser("tok");
        user.ConfirmEmail("tok");
        typeof(UserCredential).GetProperty(nameof(UserCredential.EmailConfirmationToken))!.SetValue(user.Credentials, "tok");
        typeof(UserCredential).GetProperty(nameof(UserCredential.EmailConfirmationExpiresAt))!.SetValue(user.Credentials, DateTime.UtcNow.AddMinutes(10));
        _userRepoMock.Setup(r => r.GetByEmailConfirmToken("tok", _ct)).ReturnsAsync(user);
        Func<Task> act = () => ConfirmEmailHandler.Handle(new ConfirmEmailCommand("tok"), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        await act.Should().ThrowAsync<DomainException>().WithMessage("*already confirmed*");
    }
}
