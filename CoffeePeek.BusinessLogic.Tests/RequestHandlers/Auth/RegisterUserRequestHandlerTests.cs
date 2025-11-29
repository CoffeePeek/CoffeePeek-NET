using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.Auth;

public class RegisterUserRequestHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidationStrategy<UserDto>> _validationStrategyMock;
    private readonly Mock<UserManager<CoffeePeek.Domain.Entities.Users.User>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole<int>>> _roleManagerMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly RegisterUserRequestHandler _handler;

    public RegisterUserRequestHandlerTests()
    {
        // Setup IMapper mock
        _mapperMock = new Mock<IMapper>();
        
        // Setup IValidationStrategy mock
        _validationStrategyMock = new Mock<IValidationStrategy<UserDto>>();
        
        // Setup UserManager mock
        var userStoreMock = new Mock<IUserStore<CoffeePeek.Domain.Entities.Users.User>>();
        _userManagerMock = new Mock<UserManager<CoffeePeek.Domain.Entities.Users.User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
            
        // Setup RoleManager mock
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(
            roleStoreMock.Object,
            null!, null!, null!, null!);
            
        // Setup RedisService mock
        _redisServiceMock = new Mock<IRedisService>();
        
        // Create handler instance
        _handler = new RegisterUserRequestHandler(
            _mapperMock.Object,
            _validationStrategyMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _redisServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterUserRequest("test@example.com", "Test User", "password123");
        var existingUser = new CoffeePeek.Domain.Entities.Users.User { Email = "test@example.com" };
        
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email already exists");

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(request), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenValidationFails()
    {
        // Arrange
        var request = new RegisterUserRequest("test@example.com", "Test User", "password123");
        const string validationError = "Username is required";
        var validationResult = new ValidationResult
        {
            ErrorMessage = validationError
        };
        CoffeePeek.Domain.Entities.Users.User? existingUser = null;
        
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);
            
        _mapperMock
            .Setup(m => m.Map<UserDto>(request))
            .Returns(new UserDto { Email = request.Email, UserName = request.UserName, Password = request.Password });
            
        _validationStrategyMock
            .Setup(v => v.Validate(It.IsAny<UserDto>()))
            .Returns(validationResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid request");
        result.Message.Should().Contain(validationError);

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(request), Times.Once);
        _validationStrategyMock.Verify(v => v.Validate(It.IsAny<UserDto>()), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<CoffeePeek.Domain.Entities.Users.User>(), request.Password), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserCreationFails()
    {
        // Arrange
        var request = new RegisterUserRequest("test@example.com", "Test User", "password123");
        CoffeePeek.Domain.Entities.Users.User? existingUser = null;
        var validationResult =  ValidationResult.Valid;
        var userDto = new UserDto { Email = request.Email, UserName = request.UserName, Password = request.Password };
        var user = new CoffeePeek.Domain.Entities.Users.User { Email = request.Email, UserName = request.UserName };
        var identityError = new IdentityError { Description = "Password too weak" };
        var identityResult = IdentityResult.Failed(identityError);
        
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);
            
        _mapperMock
            .Setup(m => m.Map<UserDto>(request))
            .Returns(userDto);
            
        _validationStrategyMock
            .Setup(v => v.Validate(userDto))
            .Returns(validationResult);
            
        _mapperMock
            .Setup(m => m.Map<CoffeePeek.Domain.Entities.Users.User>(userDto))
            .Returns(user);
            
        _userManagerMock
            .Setup(u => u.CreateAsync(user, request.Password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot create user");
        result.Message.Should().Contain(identityError.Description);

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(request), Times.Once);
        _validationStrategyMock.Verify(v => v.Validate(userDto), Times.Once);
        _mapperMock.Verify(m => m.Map<CoffeePeek.Domain.Entities.Users.User>(userDto), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(user, request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var request = new RegisterUserRequest("test@example.com", "Test User", "password123");
        Domain.Entities.Users.User? existingUser = null;
        var validationResult = ValidationResult.Valid;
        var userDto = new UserDto { Email = request.Email, UserName = request.UserName, Password = request.Password };
        var user = new Domain.Entities.Users.User { Id = 1, Email = request.Email, UserName = request.UserName };
        var identityResult = IdentityResult.Success;
        var registerUserResponse = new RegisterUserResponse { Email = request.Email, UserName = request.UserName };
        
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);
            
        _mapperMock
            .Setup(m => m.Map<UserDto>(request))
            .Returns(userDto);
            
        _validationStrategyMock
            .Setup(v => v.Validate(userDto))
            .Returns(validationResult);
            
        _mapperMock
            .Setup(m => m.Map<Domain.Entities.Users.User>(userDto))
            .Returns(user);
            
        _userManagerMock
            .Setup(u => u.CreateAsync(user, request.Password))
            .ReturnsAsync(identityResult);
            
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
            
        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<IdentityRole<int>>()))
            .ReturnsAsync(IdentityResult.Success);
            
        _userManagerMock
            .Setup(u => u.AddToRoleAsync(user, RoleConsts.User))
            .ReturnsAsync(IdentityResult.Success);
            
        _mapperMock
            .Setup(m => m.Map<RegisterUserResponse>(user))
            .Returns(registerUserResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be(request.Email);

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto>(request), Times.Once);
        _validationStrategyMock.Verify(v => v.Validate(userDto), Times.Once);
        _mapperMock.Verify(m => m.Map<Domain.Entities.Users.User>(userDto), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(user, request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(RoleConsts.User), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole<int>>()), Times.Exactly(1));
        _userManagerMock.Verify(u => u.AddToRoleAsync(user, RoleConsts.User), Times.Once);
        _redisServiceMock.Verify(r => r.SetAsync($"{nameof(Domain.Entities.Users.User)}{user.Id}", user), Times.Once);
        _mapperMock.Verify(m => m.Map<RegisterUserResponse>(user), Times.Once);
    }
}