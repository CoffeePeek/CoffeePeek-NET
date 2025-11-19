using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Domain.Entities.Review;

public class Review : BaseEntity
{
    public string Header { get; set; }
    public string Comment { get; set; }
    public int UserId { get; set; }
    public int ShopId { get; set; }
    public DateTime ReviewDate { get; set; }
    
    public decimal RatingCoffee { get; set; }
    public decimal RatingPlace { get; set; }
    public decimal RatingService { get; set; }

    public virtual User User { get; set; }
    public virtual Entities.Shop.Shop Shop { get; set; }
}