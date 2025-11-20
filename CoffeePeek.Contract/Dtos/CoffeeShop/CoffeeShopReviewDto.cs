namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class CoffeeShopReviewDto
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int UserId { get; set; }

    public string Header { get; set; }
    public string Comment { get; set; }

    public decimal RatingCoffee { get; set; }
    public decimal RatingService { get; set; }
    public decimal RatingPlace { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ShopName { get; set; }
}

