using CoffeePeek.Contract.Dtos;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Reviews;

public sealed class CreateCoffeeShopReviewRequest
{
    public required Guid CoffeeShopId { get; init; }
    public required bool IsPublic { get; init; }
    public required DateTime VisitedAt { get; init; }
    public string? Note { get; init; }
    public ICollection<UploadedPhotoDto>? Photos { get; init; }
    public RatingDto? Rating { get; init; }
}
