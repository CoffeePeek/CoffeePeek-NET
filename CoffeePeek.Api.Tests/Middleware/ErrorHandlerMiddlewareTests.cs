using System.Text.Json;
using CoffeePeek.Api.Middleware;
using CoffeePeek.BusinessLogic.Exceptions;
using CoffeePeek.Contract.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoffeePeek.Api.Test.Middleware;

public class ErrorHandlerMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlerMiddleware>> _loggerMock;
    private readonly ErrorHandlerMiddleware _middleware;

    public ErrorHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlerMiddleware>>();
        _middleware = new ErrorHandlerMiddleware(
            next: _ => Task.CompletedTask,
            logger: _loggerMock.Object
        );
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext_WhenNoExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new ErrorHandlerMiddleware(
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn503_WhenNpgsqlExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exceptionMessage = "Database connection failed";
        var middleware = new ErrorHandlerMiddleware(
            next: _ => throw new NpgsqlException(exceptionMessage),
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Response<object>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn404_WhenNotFoundExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exceptionMessage = "User not found";
        var middleware = new ErrorHandlerMiddleware(
            next: _ => throw new NotFoundException(exceptionMessage),
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Response<object>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn401_WhenUnauthorizedAccessExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var middleware = new ErrorHandlerMiddleware(
            next: _ => throw new UnauthorizedAccessException(),
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Response<object>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Unauthorized access.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn500_WhenUnhandledExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exceptionMessage = "Something went wrong";
        var middleware = new ErrorHandlerMiddleware(
            next: _ => throw new Exception(exceptionMessage),
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<Response<object>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("An error occurred while processing your request.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogError_WhenUnhandledExceptionThrown()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exception = new Exception("Unhandled error");
        var middleware = new ErrorHandlerMiddleware(
            next: _ => throw exception,
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled Exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(typeof(NpgsqlException), StatusCodes.Status503ServiceUnavailable)]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized)]
    public async Task InvokeAsync_ShouldReturnCorrectStatusCode_ForDifferentExceptions(Type exceptionType, int expectedStatusCode)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        Exception exception = exceptionType.Name switch
        {
            nameof(NpgsqlException) => new NpgsqlException("DB error"),
            nameof(NotFoundException) => new NotFoundException("Not found"),
            nameof(UnauthorizedAccessException) => new UnauthorizedAccessException(),
            _ => new Exception("Unknown error")
        };

        var middleware = new ErrorHandlerMiddleware(
            next: _ => throw exception,
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(expectedStatusCode);
    }
}
