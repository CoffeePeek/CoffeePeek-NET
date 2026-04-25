using System.Net;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using CoffeePeek.Contract.Dtos;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.WebClient;

public class WebModerationShopsClientTests
{
    private readonly Mock<IHttpCommandExecutor> _executorMock = new();
    private readonly FakeHttpMessageHandler _httpMessageHandler = new();

    private WebModerationShopsClient CreateSut()
    {
        var httpClient = new HttpClient(_httpMessageHandler);
        return new WebModerationShopsClient(_executorMock.Object, httpClient);
    }

    [Fact]
    public async Task SendSuggestionAsync_SendsPostToModerationShops()
    {
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute<SendCoffeeShopToModerationResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse<SendCoffeeShopToModerationResponseDto>
            {
                IsSuccess = true,
                Data = new SendCoffeeShopToModerationResponseDto { ShopId = Guid.NewGuid(), Status = "Pending" }
            });

        var sut = CreateSut();
        var result = await sut.SendSuggestionAsync(new SendCoffeeShopToModerationRequest
        {
            Name = "Test",
            Address = "Addr",
            CityId = Guid.NewGuid()
        });

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/ModerationShops");
        captured.Method.Should().Be(HttpMethod.Post);
        captured.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task UploadShopPhotosAsync_RequestsUrlsAndUploadsFiles()
    {
        _executorMock
            .Setup(e => e.Execute<List<GenerateUploadUrlResponseDto>>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<GenerateUploadUrlResponseDto>>
            {
                IsSuccess = true,
                Data =
                [
                    new GenerateUploadUrlResponseDto { UploadUrl = "https://storage.test/1", StorageKey = "shop/1.jpg" },
                    new GenerateUploadUrlResponseDto { UploadUrl = "https://storage.test/2", StorageKey = "shop/2.jpg" }
                ]
            });

        _httpMessageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK);

        var sut = CreateSut();
        var result = await sut.UploadShopPhotosAsync(
        [
            new ShopPhotoBinaryPayload("1.jpg", "image/jpeg", [1, 2, 3]),
            new ShopPhotoBinaryPayload("2.jpg", "image/jpeg", [4, 5, 6])
        ]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].StorageKey.Should().Be("shop/1.jpg");
    }

    [Fact]
    public async Task UploadShopPhotosAsync_ReturnsFailure_WhenPutFails()
    {
        _executorMock
            .Setup(e => e.Execute<List<GenerateUploadUrlResponseDto>>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<List<GenerateUploadUrlResponseDto>>
            {
                IsSuccess = true,
                Data = [new GenerateUploadUrlResponseDto { UploadUrl = "https://storage.test/1", StorageKey = "shop/1.jpg" }]
            });

        _httpMessageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        var sut = CreateSut();
        var result = await sut.UploadShopPhotosAsync(
        [
            new ShopPhotoBinaryPayload("1.jpg", "image/jpeg", [1, 2, 3])
        ]);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Photo upload failed"));
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = ResponseFactory?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK);
            return Task.FromResult(response);
        }
    }
}
