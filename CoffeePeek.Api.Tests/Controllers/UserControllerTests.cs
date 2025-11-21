using CoffeePeek.Api.Controllers;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Infrastructure.Constants;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeePeek.Api.Test.Controllers;

public class UserControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHub> _hubMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _hubMock = new Mock<IHub>();
        _controller = new UserController(_mediatorMock.Object, _hubMock.Object);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUserProfile_WhenUserAuthenticated()
    {
        // Arrange
        var userId = 1;
        var userDto = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "Test User",
            Password = "password",
            Token = "token"
        };
        var expectedResponse = new Response<UserDto>
        {
            Success = true,
            Data = userDto
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProfileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.GetProfile(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.Email.Should().Be("test@example.com");
        result.Data.UserName.Should().Be("Test User");

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetProfileRequest>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProfile_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await _controller.Invoking(c => c.GetProfile(CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetProfileRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnUpdatedProfile_WhenValidRequest()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateProfileRequest
        {
            UserId = 0,
            UserName = "Updated Name",
            About = "About me"
        };
        var expectedResponse = new Response<UpdateProfileResponse>
        {
            Success = true,
            Data = new UpdateProfileResponse()
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateProfileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Setup HttpContext with UserId BEFORE calling the controller method
        var httpContext = CreateHttpContextWithUserId(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.UpdateProfile(updateRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateProfileRequest>(r => r.UserId == userId && r.UserName == "Updated Name"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        var updateRequest = new UpdateProfileRequest
        {
            UserName = "Updated Name",
            About = "About me"
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await _controller.Invoking(c => c.UpdateProfile(updateRequest, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated or UserId is not available.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateProfileRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnUserArray_WhenCalled()
    {
        // Arrange
        var users = new[]
        {
            new UserDto
            {
                Id = 1,
                Email = "user1@example.com",
                UserName = "User 1",
                Password = "password",
                Token = "token"
            },
            new UserDto
            {
                Id = 2,
                Email = "user2@example.com",
                UserName = "User 2",
                Password = "password",
                Token = "token"
            }
        };

        var expectedResponse = new Response<UserDto[]>
        {
            Success = true,
            Data = users
        };

        var spanMock = new Mock<ISpan>();
        _hubMock.Setup(h => h.GetSpan()).Returns(spanMock.Object);
        spanMock.Setup(s => s.StartChild(It.IsAny<string>())).Returns((ISpan)null!);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllUsersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetAllUsers(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].Email.Should().Be("user1@example.com");
        result.Data[1].Email.Should().Be("user2@example.com");

        _hubMock.Verify(h => h.GetSpan(), Times.Once);
        spanMock.Verify(s => s.StartChild("additional-work"), Times.Once);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetAllUsersRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnTrue_WhenUserDeleted()
    {
        // Arrange
        var userId = 1;
        var expectedResponse = new Response<bool>
        {
            Success = true,
            Data = true
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<DeleteUserRequest>(r => r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        var userId = 999;
        var expectedResponse = new Response<bool>
        {
            Success = false,
            Message = "User not found",
            Data = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Data.Should().BeFalse();
    }

    private static HttpContext CreateHttpContextWithUserId(int userId)
    {
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                [AuthConfig.JWTTokenUserPropertyName] = userId
            }
        };
        return httpContext;
    }
}