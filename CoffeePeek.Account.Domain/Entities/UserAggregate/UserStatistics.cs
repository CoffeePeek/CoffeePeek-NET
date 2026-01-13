namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public partial record UserStatistics
{
    public int CheckInCount { get; private set; }
    public int ReviewCount { get; private set; }
    public int AddedShopsCount { get; private set; }
    public DateTime StatisticUpdatedAtUtc { get; set; } = DateTime.UtcNow;

    private UserStatistics()
    {
        
    }

    internal UserStatistics(int checkInCount, int reviewCount, int addedShopsCount)
    {
        
    }
    
    public static UserStatistics Empty() => new(0, 0, 0);
    
    public UserStatistics IncrementReviews() => this with { ReviewCount = ReviewCount + 1 };
    public UserStatistics IncrementCheckIn() => this with { CheckInCount = CheckInCount + 1 };
    public UserStatistics IncrementAddedShops() => this with { AddedShopsCount = AddedShopsCount + 1 };
}