using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Abstracts;
using CoffeePeek.Shops.Domain.CoffeeShopAggregate.Enums;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public sealed partial class CoffeeShop : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PriceRange PriceRange { get; private set; }
    public ShopStatus Status { get; private set; } = ShopStatus.Active;

    public Guid CreatorId { get; private set; }
    public Guid? ModerationId {get; private set; }

    public ShopContact Contact { get; private set; }
    public Location Location { get; private set; }

    private readonly List<ShopSchedule> _schedules = [];
    public IReadOnlyCollection<ShopSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<ShopPhoto> _shopPhotos = [];
    public IReadOnlyCollection<ShopPhoto> ShopPhotos => _shopPhotos.AsReadOnly();

    private readonly List<Equipment> _equipments = [];
    public IReadOnlyCollection<Equipment> Equipments => _equipments.AsReadOnly();

    private readonly List<CoffeeBean> _coffeeBeans = [];
    public IReadOnlyCollection<CoffeeBean> CoffeeBeans => _coffeeBeans.AsReadOnly();

    private readonly List<Roaster> _roasters = [];
    public IReadOnlyCollection<Roaster> Roasters => _roasters.AsReadOnly();

    private readonly List<BrewMethod> _brewMethods = [];
    public IReadOnlyCollection<BrewMethod> BrewMethods => _brewMethods.AsReadOnly();
    
    private readonly List<Review> _reviews = [];
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    
    private readonly List<CheckIn> _checkIns = [];
    public IReadOnlyCollection<CheckIn> CheckIns => _checkIns.AsReadOnly();

    // ReSharper disable once UnusedMember.Local
    private CoffeeShop()
    {
    }

    public CoffeeShop(Guid creatorId, string name, PriceRange priceRange, Guid moderationId)
    {
        Id = Guid.NewGuid();
        CreatorId = creatorId;
        Name = name;
        PriceRange = priceRange;
        ModerationId = moderationId;
    }
}