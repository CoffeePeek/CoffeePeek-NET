using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Moderation.Domain;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public record SendReviewToModerationCommand(
    [property: JsonIgnore] Guid UserId,
    [property: JsonIgnore] string UserName,
    Guid ShopId,
    [MaxLength(BusinessConstants.MaxReviewHeaderLength)]
    string Header,
    [MaxLength(BusinessConstants.MaxReviewCommentLength)]
    string Comment,
    RatingDto Rating,
    ICollection<UploadedPhotoDto>? Photos,
    [property: JsonIgnore] Guid? CheckInId = null);