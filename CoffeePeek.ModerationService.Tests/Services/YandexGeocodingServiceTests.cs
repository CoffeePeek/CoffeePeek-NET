using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Services;
using CoffeePeek.ModerationService.Services.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CoffeePeek.ModerationService.Tests.Services;

public class YandexGeocodingServiceTests
{
    private readonly Mock<ILogger<YandexGeocodingService>> _loggerMock;
    private readonly Mock<IOptions<YandexApiOptions>> _optionsMock;
    private readonly YandexApiOptions _options;

    public YandexGeocodingServiceTests()
    {
        _loggerMock = new Mock<ILogger<YandexGeocodingService>>();
        _optionsMock = new Mock<IOptions<YandexApiOptions>>();
        _options = new YandexApiOptions
        {
            BaseUrl = "https://geocode-maps.yandex.ru/v1/",
            ApiKey = "test-api-key"
        };
        _optionsMock.Setup(x => x.Value).Returns(_options);
    }

    [Fact]
    public async Task GeocodeAsync_WithValidAddress_ReturnsCoordinates()
    {
        // Arrange
        const string address = "Moscow, Red Square";
        const decimal expectedLat = 55.753544m;
        const decimal expectedLon = 37.621202m;

        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>
                    {
                        new()
                        {
                            GeoObject = new GeoObject
                            {
                                Point = new Point
                                {
                                    Pos = $"{expectedLon} {expectedLat}"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(expectedLat);
        result.Longitude.Should().Be(expectedLon);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GeocodeAsync_WithEmptyAddress_ReturnsNull(string? address)
    {
        // Arrange
        var httpClient = CreateHttpClient(new YandexGeocodingResponse());
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithNoResults_ReturnsNull()
    {
        // Arrange
        var address = "NonexistentPlace123456789";
        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>()
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithNullResponse_ReturnsNull()
    {
        // Arrange
        const string address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = null
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithNullGeoObjectCollection_ReturnsNull()
    {
        // Arrange
        var address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = null
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithNullPoint_ReturnsNull()
    {
        // Arrange
        const string address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>
                    {
                        new()
                        {
                            GeoObject = new GeoObject
                            {
                                Point = null
                            }
                        }
                    }
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithInvalidCoordinateFormat_ReturnsNull()
    {
        // Arrange
        const string address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>
                    {
                        new()
                        {
                            GeoObject = new GeoObject
                            {
                                Point = new Point
                                {
                                    Pos = "invalid format"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithSingleCoordinate_ReturnsNull()
    {
        // Arrange
        const string address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>
                    {
                        new()
                        {
                            GeoObject = new GeoObject
                            {
                                Point = new Point
                                {
                                    Pos = "37.621202"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithHttpError_ReturnsNull()
    {
        // Arrange
        var address = "Test Address";
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Bad Request"));

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithTimeout_ReturnsNull()
    {
        // Arrange
        var address = "Test Address";
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithSpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        const string address = "Test & Address, City / Region";
        const decimal expectedLat = 50.0m;
        const decimal expectedLon = 40.0m;

        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>
                    {
                        new()
                        {
                            GeoObject = new GeoObject
                            {
                                Point = new Point
                                {
                                    Pos = $"{expectedLon} {expectedLat}"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(expectedLat);
        result.Longitude.Should().Be(expectedLon);
    }

    [Fact]
    public async Task GeocodeAsync_WithCorrectCoordinateOrder_ParsesCorrectly()
    {
        // Arrange
        // Yandex API returns coordinates as "longitude latitude" (lon lat)
        const string address = "Moscow";
        const decimal expectedLat = 55.7558m;
        const decimal expectedLon = 37.6173m;

        var response = new YandexGeocodingResponse
        {
            Response = new GeocodingResponseData
            {
                GeoObjectCollection = new GeoObjectCollection
                {
                    FeatureMember = new List<FeatureMember>
                    {
                        new()
                        {
                            GeoObject = new GeoObject
                            {
                                Point = new Point
                                {
                                    Pos = $"{expectedLon} {expectedLat}" // lon lat order
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClient = CreateHttpClient(response);
        var sut = new YandexGeocodingService(httpClient, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(expectedLat);
        result.Longitude.Should().Be(expectedLon);
    }

    private HttpClient CreateHttpClient(YandexGeocodingResponse response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };
    }
}