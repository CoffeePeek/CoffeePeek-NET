using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ShopsService.Entities;

public class Review
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(70)] public required string Header { get; set; }
    [MaxLength(2000)] public required string Comment { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime ReviewDate { get; set; }

    public decimal RatingCoffee { get; set; }
    public decimal RatingPlace { get; set; }
    public decimal RatingService { get; set; }
    public decimal AverageRating => (RatingCoffee + RatingPlace + RatingService) / 3m;
    
    public virtual Shop Shop { get; set; }
    
}