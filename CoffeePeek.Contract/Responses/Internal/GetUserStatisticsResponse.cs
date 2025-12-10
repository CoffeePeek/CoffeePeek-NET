namespace CoffeePeek.Contract.Response.Internal;

public class GetUserStatisticsResponse(int checkInCount, int reviewCount)
{
    public int CheckInCount { get; init; } = checkInCount;
    public int ReviewCount { get; init; } = reviewCount;
}