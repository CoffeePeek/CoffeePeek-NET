using CoffeePeek.BusinessLogic.Exceptions;
using FluentAssertions;

namespace CoffeePeek.BusinessLogic.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void NotFoundException_Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Entity not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void NotFoundException_ShouldInheritFromException()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void NpgsqlException_Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Database error occurred";

        // Act
        var exception = new NpgsqlException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void NpgsqlException_ShouldInheritFromException()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new NpgsqlException(message);

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}