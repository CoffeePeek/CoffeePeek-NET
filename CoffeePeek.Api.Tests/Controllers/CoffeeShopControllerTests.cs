using System.Security.Claims;
using CoffeePeek.Api.Controllers;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeePeek.Api.Test.Controllers;

public class CoffeeShopControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CoffeeShopController _controller;

    public CoffeeShopControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CoffeeShopController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetCoffeeShops_ShouldReturnCoffeeShops_WhenAuthenticatedUser()
    {
        // Arrange
        var cityId = 1;
        var pageNumber = 1;
        var pageSize = 10;

        var coffeeShopsResponse = new GetCoffeeShopsResponse
        {
            CoffeeShopDtos = [],
            TotalItems = 10,
            TotalPages = 2,
            CurrentPage = 1,
            PageSize = 10
        };

        var expectedResponse = new Response<GetCoffeeShopsResponse>
        {
            Success = true,
            Data = coffeeShopsResponse
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCoffeeShopsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Setup authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetCoffeeShops(cityId, pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalItems.Should().Be(10);
        result.Data.TotalPages.Should().Be(2);
        result.Data.CurrentPage.Should().Be(1);
        result.Data.PageSize.Should().Be(10);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetCoffeeShopsRequest>(r =>
                    r.CityId == cityId &&
                    r.PageNumber == pageNumber &&
                    r.PageSize == pageSize),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify pagination headers
        _controller.Response.Headers.Should().ContainKey("X-Total-Count");
        _controller.Response.Headers.Should().ContainKey("X-Total-Pages");
        _controller.Response.Headers.Should().ContainKey("X-Current-Page");
        _controller.Response.Headers.Should().ContainKey("X-Page-Size");
    }

    [Fact]
    public async Task GetCoffeeShops_ShouldUseDefaultCityId_WhenUnauthenticatedUser()
    {
        // Arrange
        var requestedCityId = 5;
        var pageNumber = 1;
        var pageSize = 10;

        var coffeeShopsResponse = new GetCoffeeShopsResponse
        {
            CoffeeShopDtos = [],
            TotalItems = 5,
            TotalPages = 1,
            CurrentPage = 1,
            PageSize = 10
        };

        var expectedResponse = new Response<GetCoffeeShopsResponse>
        {
            Success = true,
            Data = coffeeShopsResponse
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCoffeeShopsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Setup unauthenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetCoffeeShops(requestedCityId, pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetCoffeeShopsRequest>(r =>
                    r.CityId == 1 && // Default unauthorized city ID
                    r.PageNumber == pageNumber &&
                    r.PageSize == pageSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCoffeeShops_ShouldSetPaginationHeaders_WhenDataReturned()
    {
        // Arrange
        var cityId = 1;
        var coffeeShopsResponse = new GetCoffeeShopsResponse
        {
            CoffeeShopDtos = [],
            TotalItems = 100,
            TotalPages = 10,
            CurrentPage = 2,
            PageSize = 10
        };

        var expectedResponse = new Response<GetCoffeeShopsResponse>
        {
            Success = true,
            Data = coffeeShopsResponse
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCoffeeShopsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        await _controller.GetCoffeeShops(cityId, 2, 10);

        // Assert
        _controller.Response.Headers["X-Total-Count"].ToString().Should().Be("100");
        _controller.Response.Headers["X-Total-Pages"].ToString().Should().Be("10");
        _controller.Response.Headers["X-Current-Page"].ToString().Should().Be("2");
        _controller.Response.Headers["X-Page-Size"].ToString().Should().Be("10");
    }

    [Fact]
    public async Task GetCoffeeShops_ShouldUseDefaultPagination_WhenNotProvided()
    {
        // Arrange
        var cityId = 1;
        var expectedResponse = new Response<GetCoffeeShopsResponse>
        {
            Success = true,
            Data = new GetCoffeeShopsResponse
            {
                CoffeeShopDtos = [],
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = 1,
                PageSize = 10
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCoffeeShopsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        await _controller.GetCoffeeShops(cityId);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetCoffeeShopsRequest>(r =>
                    r.PageNumber == 1 &&
                    r.PageSize == 10),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}