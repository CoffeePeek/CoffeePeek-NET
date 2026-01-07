using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities;

public sealed class Shop : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PriceRange PriceRange { get; private set; }
    public Guid CityId { get; private set; }

    public Guid? ShopContactId { get; private set; }
    public Guid? LocationId { get; private set; }
    public Guid CreatorId { get; private set; }
    public Guid? ModerationId {get; private set; }

    public ShopContact? ShopContact { get; private set; }
    public Location? Location { get; private set; }

    private readonly List<ShopSchedule> _schedules = [];
    public IReadOnlyCollection<ShopSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<ShopPhoto> _shopPhotos = [];
    public IReadOnlyCollection<ShopPhoto> ShopPhotos => _shopPhotos.AsReadOnly();

    private readonly List<ShopEquipment> _shopEquipments = [];
    public IReadOnlyCollection<ShopEquipment> ShopEquipments => _shopEquipments.AsReadOnly();

    private readonly List<CoffeeBeanShop> _coffeeBeanShops = [];
    public IReadOnlyCollection<CoffeeBeanShop> CoffeeBeanShops => _coffeeBeanShops.AsReadOnly();

    private readonly List<RoasterShop> _roasterShops = [];
    public IReadOnlyCollection<RoasterShop> RoasterShops => _roasterShops.AsReadOnly();

    private readonly List<ShopBrewMethod> _shopBrewMethods = [];
    public IReadOnlyCollection<ShopBrewMethod> ShopBrewMethods => _shopBrewMethods.AsReadOnly();

    public ICollection<Review> Reviews { get; private set; } = new HashSet<Review>();
    public ICollection<CheckIn> CheckIns { get; private set; } = new HashSet<CheckIn>();

    // ReSharper disable once UnusedMember.Local
    private Shop()
    {
    }

    public Shop(Guid creatorId, string name, Guid cityId, PriceRange priceRange, Guid moderationId)
    {
        Id = Guid.NewGuid();
        CreatorId = creatorId;
        Name = name;
        CityId = cityId;
        PriceRange = priceRange;
        ModerationId = moderationId;
    }

    #region Domain Logic

    public void UpdateDetails(string name, string? description, PriceRange priceRange)
    {
        Name = name;
        Description = description;
        PriceRange = priceRange;
    }

    public void SetLocation(Location location)
    {
        Location = location;
        LocationId = Location.Id;
    }

    public void SetContact(ShopContactDto contact, Guid shopId)
    {
        ShopContact = new ShopContact
        {
            ShopId = shopId,
            InstagramLink = contact.InstagramLink,
            Email = contact.Email,
            SiteLink = contact.SiteLink,
            PhoneNumber = contact.PhoneNumber,
        };
        ShopContactId = ShopContact.Id;
    }
    
    public void AddPhotos(IEnumerable<ShopPhoto> photos)
    {
        _shopPhotos.AddRange(photos);
    }
    
    public void SetEquipment(IEnumerable<Guid> equipmentIds)
    {
        _shopEquipments.Clear();
        foreach (var equipmentId in equipmentIds)
        {
            _shopEquipments.Add(new ShopEquipment(equipmentId, Id));
        }
    }
    
    public void SetBrewMethods(IEnumerable<Guid> brewMethodIds)
    {
        _shopBrewMethods.Clear();
        foreach (var brewMethodId in brewMethodIds)
        {
            _shopBrewMethods.Add(new ShopBrewMethod(brewMethodId, Id));
        }
    }
    
    public void SetRoasters(IEnumerable<Guid> roasterIds)
    {
        _roasterShops.Clear();
        foreach (var roasterId in roasterIds)
        {
            _roasterShops.Add(new RoasterShop(roasterId, Id));
        }
    }
    
    public void SetBeans(IEnumerable<Guid> coffeeBeanIds)
    {
        _coffeeBeanShops.Clear();
        foreach (var coffeeBeanId in coffeeBeanIds)
        {
            _coffeeBeanShops.Add(new CoffeeBeanShop(coffeeBeanId, Id));
        }
    }
    #endregion
}