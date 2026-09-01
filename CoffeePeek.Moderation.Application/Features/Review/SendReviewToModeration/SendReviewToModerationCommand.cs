using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Moderation.Domain;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public record SendReviewToModerationCommand(
    Guid ShopId,
    [MaxLength(BusinessConstants.MaxReviewHeaderLength)]
    string Header,
    [MaxLength(BusinessConstants.MaxReviewCommentLength)]
    string Comment,
    RatingDto? Rating,
    ICollection<UploadedPhotoDto>? Photos)
{
    [JsonIgnore] public Guid UserId { get; init; }
    [JsonIgnore] public string UserName { get; init; } = string.Empty;
    [JsonIgnore] public Guid? CheckInId { get; init; }

    [JsonPropertyName("ratingCoffee")] public int? RatingCoffee { get; init; }
    [JsonPropertyName("ratingPlace")] public int? RatingPlace { get; init; }
    [JsonPropertyName("ratingService")] public int? RatingService { get; init; }

    /// <summary>
    /// Reconciles the two rating shapes the live client and internal consumers send: a nested
    /// <see cref="Rating"/> object (legacy/internal, e.g. <c>CheckInCreatedHandler</c>) or flat
    /// top-level <see cref="RatingCoffee"/>/<see cref="RatingPlace"/>/<see cref="RatingService"/>
    /// fields (the actual live client payload). Validation and persistence should always read
    /// from this property, never from <see cref="Rating"/> directly.
    /// </summary>
    [JsonIgnore]
    public RatingDto EffectiveRating => Rating ?? new RatingDto
    {
        Coffee = RatingCoffee ?? 0,
        Place = RatingPlace ?? 0,
        Service = RatingService ?? 0
    };
}
