using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

public sealed partial class Review : Entity<Guid>
{
    public string Header { get; private set; }
    public string Comment { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    public DateTime ReviewDate { get; private set; }

    public bool IsSoftDelete { get; private set; }

    public int RatingCoffee { get; private set; }
    public int RatingPlace { get; private set; }
    public int RatingService { get; private set; }
    public decimal AverageRating => (RatingCoffee + RatingPlace + RatingService) / 3m;
    
    public CoffeeShop? Shop { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private Review()
    {
    }

    private Review(Guid shopId, Guid userId, string header, string comment)
    {
        Id = Guid.NewGuid();
        ShopId = shopId;
        UserId = userId;
        Header = header;
        Comment = comment;
        ReviewDate = DateTime.UtcNow;
    }

    private Review(Guid shopId, Guid userId, string header, string comment, int ratingCoffee, int ratingPlace,
        int ratingService)
        : this(shopId, userId, header, comment)
    {
        RatingCoffee = ratingCoffee;
        RatingPlace = ratingPlace;
        RatingService = ratingService;
    }
    
}