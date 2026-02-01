namespace CoffeePeek.Contract.Dtos;

public record RatingDto
{
    public int Place { get; init; }
    public int Service { get; init; }
    public int Coffee { get; init; }
}