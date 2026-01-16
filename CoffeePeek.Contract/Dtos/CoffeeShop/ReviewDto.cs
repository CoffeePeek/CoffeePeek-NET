namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public record ReviewDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    public string Header { get; init; }
    public string Comment { get; init; }

    public int RatingCoffee { get; init; }
    public int RatingService { get; init; }
    public int RatingPlace { get; init; }

    public DateTime CreatedAt { get; init; }
}

