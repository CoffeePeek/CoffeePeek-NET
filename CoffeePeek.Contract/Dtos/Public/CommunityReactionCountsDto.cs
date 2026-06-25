namespace CoffeePeek.Contract.Dtos.Public;

public record CommunityReactionCountsDto
{
    public int WantToTry { get; init; }
    public int GreatFind { get; init; }
    public int Helpful { get; init; }
}
