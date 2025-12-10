using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class CheckInReviewRequest
{
    [Required]
    [MaxLength(70)]
    public string Header { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Comment { get; init; }
    
    [Range(1, 5)]
    public int? RatingCoffee { get; init; }
    
    [Range(1, 5)]
    public int? RatingPlace { get; init; }
    
    [Range(1, 5)]
    public int? RatingService { get; init; }
}