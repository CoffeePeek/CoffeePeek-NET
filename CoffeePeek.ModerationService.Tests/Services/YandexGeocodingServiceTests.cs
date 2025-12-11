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
            BaseUrl = "https://geocode-maps.yandex.ru/1.x/",
            ApiKey = "test-api-key"
        };
        _optionsMock.Setup(x => x.Value).Returns(_options);
    }

    [Fact]
    public async Task GeocodeAsync_WithValidAddress_ReturnsCoordinates()
    {
        // Arrange
        var address = "Moscow, Red Square";
        var expectedLat = 55.753544m;
        var expectedLon = 37.621202m;

        var response = new YandexGeocodingResponse
        {
            Response = new YandexGeocodingResponse.ResponseData
            {
                GeoObjectCollection = new YandexGeocodingResponse.GeoObjectCollectionData
                {
                    FeatureMember = new List<YandexGeocodingResponse.FeatureMemberData>
                    {
                        new()
                        {
                            GeoObject = new YandexGeocodingResponse.GeoObjectData
                            {
                                Point = new YandexGeocodingResponse.PointData
                                {
                                    Pos = $"{expectedLon} {expectedLat}"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClientMock = CreateHttpClientMock(response);
        var sut = new YandexGeocodingService(httpClientMock.Object, _optionsMock.Object, _loggerMock.Object);

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
    public async Task GeocodeAsync_WithEmptyAddress_ReturnsNull(string address)
    {
        // Arrange
        var httpClientMock = CreateHttpClientMock(new YandexGeocodingResponse());
        var sut = new YandexGeocodingService(httpClientMock.Object, _optionsMock.Object, _loggerMock.Object);

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
            Response = new YandexGeocodingResponse.ResponseData
            {
                GeoObjectCollection = new YandexGeocodingResponse.GeoObjectCollectionData
                {
                    FeatureMember = new List<YandexGeocodingResponse.FeatureMemberData>()
                }
            }
        };

        var httpClientMock = CreateHttpClientMock(response);
        var sut = new YandexGeocodingService(httpClientMock.Object, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithInvalidResponse_ReturnsNull()
    {
        // Arrange
        var address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = null
        };

        var httpClientMock = CreateHttpClientMock(response);
        var sut = new YandexGeocodingService(httpClientMock.Object, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithInvalidCoordinateFormat_ReturnsNull()
    {
        // Arrange
        var address = "Test Address";
        var response = new YandexGeocodingResponse
        {
            Response = new YandexGeocodingResponse.ResponseData
            {
                GeoObjectCollection = new YandexGeocodingResponse.GeoObjectCollectionData
                {
                    FeatureMember = new List<YandexGeocodingResponse.FeatureMemberData>
                    {
                        new()
                        {
                            GeoObject = new YandexGeocodingResponse.GeoObjectData
                            {
                                Point = new YandexGeocodingResponse.PointData
                                {
                                    Pos = "invalid format"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClientMock = CreateHttpClientMock(response);
        var sut = new YandexGeocodingService(httpClientMock.Object, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_WithSpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var address = "Test & Address, City / Region";
        var expectedLat = 50.0m;
        var expectedLon = 40.0m;

        var response = new YandexGeocodingResponse
        {
            Response = new YandexGeocodingResponse.ResponseData
            {
                GeoObjectCollection = new YandexGeocodingResponse.GeoObjectCollectionData
                {
                    FeatureMember = new List<YandexGeocodingResponse.FeatureMemberData>
                    {
                        new()
                        {
                            GeoObject = new YandexGeocodingResponse.GeoObjectData
                            {
                                Point = new YandexGeocodingResponse.PointData
                                {
                                    Pos = $"{expectedLon} {expectedLat}"
                                }
                            }
                        }
                    }
                }
            }
        };

        var httpClientMock = CreateHttpClientMock(response);
        var sut = new YandexGeocodingService(httpClientMock.Object, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.GeocodeAsync(address);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(expectedLat);
        result.Longitude.Should().Be(expectedLon);
    }

    private Mock<HttpClient> CreateHttpClientMock(YandexGeocodingResponse response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = JsonSerializer.Serialize(response);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(handlerMock.Object);
        var httpClientMock = new Mock<HttpClient>();
        return httpClientMock;
    }
}