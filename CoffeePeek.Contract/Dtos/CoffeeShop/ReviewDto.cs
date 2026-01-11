namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ReviewDto
{
    public Guid UserId { get; set; }

    public string Header { get; set; }
    public string Comment { get; set; }

    public int RatingCoffee { get; set; }
    public int RatingService { get; set; }
    public int RatingPlace { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ShopName { get; set; }
}

