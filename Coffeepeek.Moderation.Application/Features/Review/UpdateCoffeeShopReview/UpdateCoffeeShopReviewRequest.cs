using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;
using CoffeePeek.Moderation.Domain;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.UpdateCoffeeShopReview;

public record UpdateCoffeeShopReviewRequest(
    [property:JsonIgnore]Guid UserId,
    Guid ReviewId,
    string Header,
    string Comment,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate, ErrorMessage = "RatingCoffee must be between 1 and 5")]
    int RatingCoffee,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate,
        ErrorMessage = "RatingService must be between 1 and 5")]
    int RatingService,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate,
        ErrorMessage = "RatingPlace must be between 1 and 5")]
    int RatingPlace) : IRequest<Response<UpdateCoffeeShopReviewResponse>>;