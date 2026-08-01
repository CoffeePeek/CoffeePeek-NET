using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.Auth.Password.ChangePassword;
using CoffeePeek.Account.Application.Features.Auth.Password.ForgotPassword;
using CoffeePeek.Account.Application.Features.Auth.Password.ResetPassword;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.Auth.Password;

public class PasswordHandlersTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static DomainUser CreatePasswordUser(string email = "user@example.com", string hash = "hash") =>
        DomainUser.Register(email, "testuser", hash, Role.Create("User"));

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_Succeeds()
    {
        var user = CreatePasswordUser();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("hash", "oldpass123")).Returns(true);
        _hasherMock.Setup(h => h.HashPassword("newpass123")).Returns("new_hash");

        var result = await ChangePasswordHandler.Handle(
            new ChangePasswordCommand(user.Id, "oldpass123", "newpass123"),
            _userRepoMock.Object, _hasherMock.Object, _unitOfWorkMock.Object, _ct);

        result.IsSuccess.Should().BeTrue();
        user.Credentials.PasswordHash.Should().Be("new_hash");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Throws()
    {
        var user = CreatePasswordUser();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("hash", "wrong")).Returns(false);

        Func<Task> act = () => ChangePasswordHandler.Handle(
            new ChangePasswordCommand(user.Id, "wrong", "newpass123"),
            _userRepoMock.Object, _hasherMock.Object, _unitOfWorkMock.Object, _ct);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Current password*");
    }

    [Fact]
    public async Task ChangePassword_WithShortNewPassword_Throws()
    {
        var user = CreatePasswordUser();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);

        Func<Task> act = () => ChangePasswordHandler.Handle(
            new ChangePasswordCommand(user.Id, "oldpass123", "short"),
            _userRepoMock.Object, _hasherMock.Object, _unitOfWorkMock.Object, _ct);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*at least 8*");
    }

    [Fact]
    public async Task ForgotPassword_WhenUserNotFound_ReturnsSilentSuccessWithoutEvent()
    {
        _userRepoMock.Setup(r => r.GetByEmail("missing@example.com", _ct)).ReturnsAsync((DomainUser?)null);

        var (response, @event) = await ForgotPasswordHandler.Handle(
            new ForgotPasswordCommand("missing@example.com"),
            _userRepoMock.Object, _unitOfWorkMock.Object, _ct);

        response.IsSuccess.Should().BeTrue();
        @event.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_WhenOAuthOnly_ReturnsSilentSuccessWithoutEvent()
    {
        var user = DomainUser.CreateExternal("oauth@example.com", "google", "gid");
        _userRepoMock.Setup(r => r.GetByEmail("oauth@example.com", _ct)).ReturnsAsync(user);

        var (response, @event) = await ForgotPasswordHandler.Handle(
            new ForgotPasswordCommand("oauth@example.com"),
            _userRepoMock.Object, _unitOfWorkMock.Object, _ct);

        response.IsSuccess.Should().BeTrue();
        @event.Should().BeNull();
    }

    [Fact]
    public async Task ForgotPassword_WhenPasswordUser_BeginsResetAndReturnsEvent()
    {
        var user = CreatePasswordUser();
        _userRepoMock.Setup(r => r.GetByEmail("user@example.com", _ct)).ReturnsAsync(user);

        var (response, @event) = await ForgotPasswordHandler.Handle(
            new ForgotPasswordCommand("user@example.com"),
            _userRepoMock.Object, _unitOfWorkMock.Object, _ct);

        response.IsSuccess.Should().BeTrue();
        @event.Should().NotBeNull();
        @event!.ResetToken.Should().Be(user.Credentials.PasswordResetToken);
        user.Credentials.PasswordResetToken.Should().NotBeNullOrEmpty();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_Succeeds()
    {
        var user = CreatePasswordUser();
        user.BeginPasswordReset();
        var token = user.Credentials.PasswordResetToken!;
        _userRepoMock.Setup(r => r.GetByPasswordResetToken(token, _ct)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.HashPassword("newpass123")).Returns("new_hash");

        var result = await ResetPasswordHandler.Handle(
            new ResetPasswordCommand(token, "newpass123"),
            _userRepoMock.Object, _hasherMock.Object, _unitOfWorkMock.Object, _ct);

        result.IsSuccess.Should().BeTrue();
        user.Credentials.PasswordHash.Should().Be("new_hash");
        user.Credentials.PasswordResetToken.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_WithUnknownToken_ThrowsNotFound()
    {
        _userRepoMock.Setup(r => r.GetByPasswordResetToken(It.IsAny<string>(), _ct)).ReturnsAsync((DomainUser?)null);

        Func<Task> act = () => ResetPasswordHandler.Handle(
            new ResetPasswordCommand("bad", "newpass123"),
            _userRepoMock.Object, _hasherMock.Object, _unitOfWorkMock.Object, _ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
