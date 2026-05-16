using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.User.DeleteUser;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.User.DeleteUser;

public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static DomainUser CreateUser()
    {
        var role = Role.Create("User");
        return DomainUser.Register("user@example.com", "testuser", "hash", role);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsSuccess()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        var result = await DeleteUserHandler.Handle(new DeleteUserCommand(user.Id), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserExists_SetsSoftDelete()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        await DeleteUserHandler.Handle(new DeleteUserCommand(user.Id), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        user.IsSoftDelete.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserExists_CallsUpdateAndSaveChanges()
    {
        var user = CreateUser();
        _userRepoMock.Setup(r => r.GetById(user.Id, _ct)).ReturnsAsync(user);
        await DeleteUserHandler.Handle(new DeleteUserCommand(user.Id), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        _userRepoMock.Verify(r => r.Update(user, _ct), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsErrorResponse()
    {
        _userRepoMock.Setup(r => r.GetById(It.IsAny<Guid>(), _ct)).ReturnsAsync((DomainUser?)null);
        var result = await DeleteUserHandler.Handle(new DeleteUserCommand(Guid.NewGuid()), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_DoesNotCallUpdateOrSave()
    {
        _userRepoMock.Setup(r => r.GetById(It.IsAny<Guid>(), _ct)).ReturnsAsync((DomainUser?)null);
        await DeleteUserHandler.Handle(new DeleteUserCommand(Guid.NewGuid()), _userRepoMock.Object, _unitOfWorkMock.Object, _ct);
        _userRepoMock.Verify(r => r.Update(It.IsAny<DomainUser>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
