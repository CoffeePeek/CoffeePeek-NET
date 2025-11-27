using CoffeePeek.Contract.Response;
using FluentAssertions;

namespace CoffeePeek.Contract.Tests.Response;

public class CreateEntityResponseTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void CreateEntityResponse_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new CreateEntityResponse<TestEntity>();

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().BeNull();
        response.Data.Should().BeNull();
        response.EntityId.Should().BeNull();
    }

    [Fact]
    public void CreateEntityResponse_ParameterizedConstructor_ShouldSetAllProperties()
    {
        // Arrange
        var success = true;
        var message = "Entity created";
        var data = new TestEntity { Id = 1, Name = "Test" };
        var entityId = 123;

        // Act
        var response = new CreateEntityResponse<TestEntity>(success, message, data, entityId);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void CreateEntityResponse_ParameterizedConstructor_WithoutEntityId_ShouldHaveNullEntityId()
    {
        // Arrange
        var success = true;
        var message = "Entity created";
        var data = new TestEntity { Id = 1, Name = "Test" };

        // Act
        var response = new CreateEntityResponse<TestEntity>(success, message, data);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.EntityId.Should().BeNull();
    }

    [Fact]
    public void CreateEntityResponse_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Success" };
        var message = "Entity created successfully";
        var entityId = 456;

        // Act
        var response = CreateEntityResponse<TestEntity>.SuccessResponse(data, message, entityId);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void CreateEntityResponse_SuccessResponse_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Test" };
        var entityId = 789;

        // Act
        var response = CreateEntityResponse<TestEntity>.SuccessResponse(data, entityId: entityId);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Entity created successfully");
        response.Data.Should().Be(data);
        response.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void CreateEntityResponse_SuccessResponse_WithoutEntityId_ShouldHaveNullEntityId()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Test" };

        // Act
        var response = CreateEntityResponse<TestEntity>.SuccessResponse(data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.EntityId.Should().BeNull();
    }

    [Fact]
    public void CreateEntityResponse_ErrorResponse_ShouldCreateErrorResponse()
    {
        // Arrange
        var message = "Failed to create entity";
        var data = new TestEntity { Id = 0, Name = "Error" };

        // Act
        var response = CreateEntityResponse<TestEntity>.ErrorResponse(message, data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.EntityId.Should().BeNull();
    }

    [Fact]
    public void CreateEntityResponse_ErrorResponse_WithoutData_ShouldHaveDefaultData()
    {
        // Arrange
        var message = "Failed to create entity";

        // Act
        var response = CreateEntityResponse<TestEntity>.ErrorResponse(message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.EntityId.Should().BeNull();
    }

    [Fact]
    public void CreateEntityResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new CreateEntityResponse<TestEntity>();
        var data = new TestEntity { Id = 1, Name = "Test" };

        // Act
        response.Success = true;
        response.Message = "Created";
        response.Data = data;
        response.EntityId = 999;

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Created");
        response.Data.Should().Be(data);
        response.EntityId.Should().Be(999);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    public void CreateEntityResponse_SuccessResponse_ShouldHandleVariousEntityIds(int entityId)
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Test" };

        // Act
        var response = CreateEntityResponse<TestEntity>.SuccessResponse(data, entityId: entityId);

        // Assert
        response.EntityId.Should().Be(entityId);
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void CreateEntityResponse_WithNullData_ShouldAllowNull()
    {
        // Act
        var response = new CreateEntityResponse<TestEntity?>
        {
            Success = true,
            Message = "Success with null data",
            Data = null,
            EntityId = 1
        };

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.EntityId.Should().Be(1);
    }
}
