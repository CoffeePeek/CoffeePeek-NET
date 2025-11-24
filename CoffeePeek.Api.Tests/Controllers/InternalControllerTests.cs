using CoffeePeek.Api.Controllers;
using CoffeePeek.Contract.Requests.Internal;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Internal;
using FluentAssertions;
using MediatR;
using Moq;

namespace CoffeePeek.Api.Test.Controllers;

public class InternalControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly InternalController _controller;

    public InternalControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new InternalController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetCities_ShouldReturnCities_WhenCalled()
    {
        // Arrange
        var expectedResponse = new Response<GetCitiesResponse>
        {
            Success = true,
            Data = new GetCitiesResponse([])
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCities();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task GetCities_ShouldReturnEmptyList_WhenNoCitiesExist()
    {
        // Arrange
        var expectedResponse = new Response<GetCitiesResponse>
        {
            Success = true,
            Data = new GetCitiesResponse([])
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCities();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCities_ShouldReturnError_WhenExceptionOccurs()
    {
        // Arrange
        var expectedResponse = new Response<GetCitiesResponse>
        {
            Success = false,
            Message = "An error occurred"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCities();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCities_ShouldCallMediatorWithCorrectRequest()
    {
        // Arrange
        var expectedResponse = new Response<GetCitiesResponse>
        {
            Success = true,
            Data = new GetCitiesResponse([])
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.GetCities();

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetCitiesRequest>(r => r != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}