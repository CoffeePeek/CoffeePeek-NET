using FluentAssertions;
using CoffeePeek.Contract.Response;

namespace CoffeePeek.Contract.Tests.Response;

public class ResponseTests
{
    [Fact]
    public void Response_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new Contract.Response.Response();

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().BeNull();
        response.Data.Should().BeNull();
    }

    [Fact]
    public void Response_ParameterizedConstructor_ShouldSetProperties()
    {
        // Arrange
        var success = true;
        var message = "Test message";
        var data = new { Id = 1, Name = "Test" };

        // Act
        var response = new Contract.Response.Response(success, message, data);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void Response_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Arrange
        var data = new { Id = 1 };
        var message = "Success!";

        // Act
        var response = Contract.Response.Response.SuccessResponse<Contract.Response.Response>(data, message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void Response_SuccessResponse_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var data = new { Id = 1 };

        // Act
        var response = Contract.Response.Response.SuccessResponse<Contract.Response.Response>(data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Operation successful");
        response.Data.Should().Be(data);
    }

    [Fact]
    public void Response_ErrorResponse_ShouldCreateErrorResponse()
    {
        // Arrange
        var message = "Error occurred";
        var data = new { Error = "Details" };

        // Act
        var response = Contract.Response.Response.ErrorResponse<Contract.Response.Response>(message, data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void Response_ErrorResponse_WithoutData_ShouldHaveNullData()
    {
        // Arrange
        var message = "Error occurred";

        // Act
        var response = Contract.Response.Response.ErrorResponse<Contract.Response.Response>(message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
    }
}

public class ResponseGenericTests
{
    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void ResponseGeneric_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new Response<TestData>();

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().BeNull();
        response.Data.Should().BeNull();
    }

    [Fact]
    public void ResponseGeneric_ParameterizedConstructor_ShouldSetProperties()
    {
        // Arrange
        var success = true;
        var message = "Test message";
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var response = new Response<TestData>(success, message, data);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.Data.Id.Should().Be(1);
        response.Data.Name.Should().Be("Test");
    }

    [Fact]
    public void ResponseGeneric_Data_ShouldBeTypeSafe()
    {
        // Arrange
        var data = new TestData { Id = 5, Name = "TypeSafe" };
        var response = new Response<TestData> { Data = data };

        // Act & Assert
        response.Data.Should().BeOfType<TestData>();
        response.Data.Id.Should().Be(5);
        response.Data.Name.Should().Be("TypeSafe");
    }

    [Fact]
    public void ResponseGeneric_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Success" };
        var message = "Success!";

        // Act
        var response = Response<TestData>.SuccessResponse<Response<TestData>>(data, message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void ResponseGeneric_SuccessResponse_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var response = Response<TestData>.SuccessResponse<Response<TestData>>(data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Operation successful");
        response.Data.Should().Be(data);
    }

    [Fact]
    public void ResponseGeneric_ErrorResponse_ShouldCreateErrorResponse()
    {
        // Arrange
        var message = "Error occurred";
        var data = new TestData { Id = 0, Name = "Error" };

        // Act
        var response = Response<TestData>.ErrorResponse<Response<TestData>>(message, data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void ResponseGeneric_ErrorResponse_WithoutData_ShouldHaveDefaultData()
    {
        // Arrange
        var message = "Error occurred";

        // Act
        var response = Response<TestData>.ErrorResponse<Response<TestData>>(message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
    }

    [Fact]
    public void ResponseGeneric_ShouldInheritFromResponse()
    {
        // Arrange & Act
        var response = new Response<TestData>();

        // Assert
        response.Should().BeAssignableTo<Contract.Response.Response>();
    }

    [Theory]
    [InlineData(true, "Success")]
    [InlineData(false, "Failure")]
    public void ResponseGeneric_ShouldHandleDifferentSuccessStates(bool success, string message)
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var response = new Response<TestData>(success, message, data);

        // Assert
        response.Success.Should().Be(success);
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void ResponseGeneric_WithNullData_ShouldAllowNull()
    {
        // Act
        var response = new Response<TestData?>
        {
            Success = true,
            Message = "Success with null data",
            Data = null
        };

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
    }
}
