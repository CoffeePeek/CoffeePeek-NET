namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ReviewDto
{
    public Guid UserId { get; set; }

    public string Header { get; set; }
    public string Comment { get; set; }

    public decimal RatingCoffee { get; set; }
    public decimal RatingService { get; set; }
    public decimal RatingPlace { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ShopName { get; set; }
}

