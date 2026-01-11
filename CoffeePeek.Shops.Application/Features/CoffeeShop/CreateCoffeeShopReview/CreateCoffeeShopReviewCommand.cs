using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CreateCoffeeShopReview;

public record CreateCoffeeShopReviewCommand(
    Guid ShopId,
    [MinLength(BusinessConstants.MinReviewHeaderLength), MaxLength(BusinessConstants.MaxReviewHeaderLength)]
    string Header,
    [MinLength(BusinessConstants.MinReviewCommentLength), MaxLength(BusinessConstants.MaxReviewCommentLength)]
    string Comment,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate,
        ErrorMessage = "{0} must be between {1} and {2}")]
    int RatingCoffee,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate,
        ErrorMessage = "{0} must be between {1} and {2}")]
    int RatingService,
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate,
        ErrorMessage = "{0} must be between {1} and {2}")]
    int RatingPlace) : IRequest<Response<CreateCoffeeShopReviewResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; }
}