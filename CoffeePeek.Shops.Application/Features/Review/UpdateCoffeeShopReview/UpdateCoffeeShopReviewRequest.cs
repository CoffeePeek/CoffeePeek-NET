using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.UpdateCoffeeShopReview;

public class UpdateCoffeeShopReviewRequest : IRequest<Response<UpdateCoffeeShopReviewResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public Guid ReviewId { get; set; }
    
    public string Header { get; set; }
    public string Comment { get; set; }
    
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate, ErrorMessage = "RatingCoffee must be between 1 and 5")]
    public int RatingCoffee { get; set; }
    
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate, ErrorMessage = "RatingService must be between 1 and 5")]
    public int RatingService { get; set; }
    
    [Range(BusinessConstants.MinReviewRate, BusinessConstants.MaxReviewRate, ErrorMessage = "RatingPlace must be between 1 and 5")]
    public int RatingPlace { get; set; }
}