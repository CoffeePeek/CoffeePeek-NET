using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public record SendReviewToModerationCommand(
    Guid ShopId,
    [MaxLength(BusinessConstants.MaxReviewHeaderLength)]
    string Header,
    [MaxLength(BusinessConstants.MaxReviewCommentLength)]
    string Comment,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate)]
    int RatingService,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate)]
    int RatingPlace,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate)]
    int RatingCoffee) : IRequest<CreateEntityResponse>
{
    [JsonIgnore] public Guid UserId { get; set; }
}