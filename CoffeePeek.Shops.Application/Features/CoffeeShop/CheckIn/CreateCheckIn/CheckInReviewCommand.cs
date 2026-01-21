using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shops.Domain;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.CreateCheckIn;

public record CheckInReviewCommand(
    [Required, MinLength(BusinessConstants.MinReviewHeaderLength),
     MaxLength(BusinessConstants.MaxReviewHeaderLength)]
    string Header,
    [Required, MinLength(BusinessConstants.MinReviewCommentLength),
     MaxLength(BusinessConstants.MaxReviewCommentLength)]
    string Comment,
    [Required, Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate)]
    int RatingCoffee,
    [Required, Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate)]
    int RatingPlace,
    [Required, Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate)]
    int RatingService);