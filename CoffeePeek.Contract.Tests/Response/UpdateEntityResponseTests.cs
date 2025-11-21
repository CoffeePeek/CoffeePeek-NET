using CoffeePeek.Contract.Response;
using FluentAssertions;

namespace CoffeePeek.Contract.Tests.Response;

public class UpdateEntityResponseTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void UpdateEntityResponse_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var response = new UpdateEntityResponse<TestEntity>();

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().BeNull();
        response.Data.Should().BeNull();
        response.OldEntity.Should().BeNull();
    }

    [Fact]
    public void UpdateEntityResponse_ParameterizedConstructor_ShouldSetAllProperties()
    {
        // Arrange
        var success = true;
        var message = "Entity updated";
        var data = new TestEntity { Id = 1, Name = "Updated" };
        var oldEntity = new TestEntity { Id = 1, Name = "Original" };

        // Act
        var response = new UpdateEntityResponse<TestEntity>(success, message, data, oldEntity);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.Data.Name.Should().Be("Updated");
        response.OldEntity.Should().Be(oldEntity);
        response.OldEntity!.Name.Should().Be("Original");
    }

    [Fact]
    public void UpdateEntityResponse_ParameterizedConstructor_WithoutOldEntity_ShouldHaveNullOldEntity()
    {
        // Arrange
        var success = true;
        var message = "Entity updated";
        var data = new TestEntity { Id = 1, Name = "Updated" };

        // Act
        var response = new UpdateEntityResponse<TestEntity>(success, message, data);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.OldEntity.Should().BeNull();
    }

    [Fact]
    public void UpdateEntityResponse_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Updated" };
        var oldEntity = new TestEntity { Id = 1, Name = "Original" };
        var message = "Update successful";

        // Act
        var response = UpdateEntityResponse<TestEntity>.SuccessResponse(data, message, oldEntity);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.OldEntity.Should().Be(oldEntity);
    }

    [Fact]
    public void UpdateEntityResponse_SuccessResponse_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Updated" };
        var oldEntity = new TestEntity { Id = 1, Name = "Original" };

        // Act
        var response = UpdateEntityResponse<TestEntity>.SuccessResponse(data, oldEntity: oldEntity);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Entity updated successfully");
        response.Data.Should().Be(data);
        response.OldEntity.Should().Be(oldEntity);
    }

    [Fact]
    public void UpdateEntityResponse_SuccessResponse_WithoutOldEntity_ShouldHaveNullOldEntity()
    {
        // Arrange
        var data = new TestEntity { Id = 1, Name = "Updated" };

        // Act
        var response = UpdateEntityResponse<TestEntity>.SuccessResponse(data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.OldEntity.Should().BeNull();
    }

    [Fact]
    public void UpdateEntityResponse_ErrorResponse_ShouldCreateErrorResponse()
    {
        // Arrange
        var message = "Failed to update entity";
        var data = new TestEntity { Id = 0, Name = "Error" };

        // Act
        var response = UpdateEntityResponse<TestEntity>.ErrorResponse(message, data);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.OldEntity.Should().BeNull();
    }

    [Fact]
    public void UpdateEntityResponse_ErrorResponse_WithoutData_ShouldHaveDefaultData()
    {
        // Arrange
        var message = "Failed to update entity";

        // Act
        var response = UpdateEntityResponse<TestEntity>.ErrorResponse(message);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.OldEntity.Should().BeNull();
    }

    [Fact]
    public void UpdateEntityResponse_Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new UpdateEntityResponse<TestEntity>();
        var data = new TestEntity { Id = 1, Name = "New" };
        var oldEntity = new TestEntity { Id = 1, Name = "Old" };

        // Act
        response.Success = true;
        response.Message = "Updated";
        response.Data = data;
        response.OldEntity = oldEntity;

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Updated");
        response.Data.Should().Be(data);
        response.OldEntity.Should().Be(oldEntity);
    }

    [Fact]
    public void UpdateEntityResponse_ShouldTrackChanges()
    {
        // Arrange
        var oldEntity = new TestEntity { Id = 1, Name = "Original Name" };
        var newEntity = new TestEntity { Id = 1, Name = "Updated Name" };

        // Act
        var response = UpdateEntityResponse<TestEntity>.SuccessResponse(newEntity, oldEntity: oldEntity);

        // Assert
        response.OldEntity!.Name.Should().Be("Original Name");
        response.Data.Name.Should().Be("Updated Name");
        response.OldEntity.Id.Should().Be(response.Data.Id);
    }

    [Fact]
    public void UpdateEntityResponse_WithNullData_ShouldAllowNull()
    {
        // Act
        var response = new UpdateEntityResponse<TestEntity?>
        {
            Success = true,
            Message = "Success with null data",
            Data = null,
            OldEntity = new TestEntity { Id = 1, Name = "Old" }
        };

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
        response.OldEntity.Should().NotBeNull();
    }

    [Fact]
    public void UpdateEntityResponse_WithBothNull_ShouldAllowBothNull()
    {
        // Act
        var response = new UpdateEntityResponse<TestEntity?>
        {
            Success = false,
            Message = "Failed",
            Data = null,
            OldEntity = null
        };

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.OldEntity.Should().BeNull();
    }
}
