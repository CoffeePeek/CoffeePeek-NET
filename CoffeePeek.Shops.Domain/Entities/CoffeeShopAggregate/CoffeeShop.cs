using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public sealed class CoffeeShop : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PriceRange PriceRange { get; private set; }
    public ShopStatus Status { get; private set; } = ShopStatus.Active;
    public Guid CityId { get; private set; }

    public Guid CreatorId { get; private set; }
    public Guid? ModerationId {get; private set; }

    public ShopContact Contact { get; private set; }
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
    
    private readonly List<Review> _reviews = [];
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    
    private readonly List<CheckIn> _checkIns = [];
    public IReadOnlyCollection<CheckIn> CheckIns => _checkIns.AsReadOnly();

    // ReSharper disable once UnusedMember.Local
    private CoffeeShop()
    {
    }

    public CoffeeShop(Guid creatorId, string name, Guid cityId, PriceRange priceRange, Guid moderationId)
    {
        Id = Guid.NewGuid();
        CreatorId = creatorId;
        Name = name;
        CityId = cityId;
        PriceRange = priceRange;
        ModerationId = moderationId;
    }

    #region Domain Logic

    public bool IsNew => CreatedAtUtc > DateTime.UtcNow.AddDays(-BusinessConstants.ItNewEntityInDays);
    public bool IsOpen => IsOpenAt(DateTime.UtcNow);
    
    public decimal Rating => Reviews.Count == 0 ? 0 : Reviews.Average(r => r.AverageRating);

    private bool IsOpenAt(DateTime dateTime)
    {
        switch (Status)
        {
            case ShopStatus.PermanentlyClosed:
            case ShopStatus.TemporarilyClosed:
                return false;
            case ShopStatus.Active:
                break;
        }

        if (Schedules.Count == 0)
            return true;
            
        var daySchedule = Schedules.FirstOrDefault(s => s.DayOfWeek == dateTime.DayOfWeek);
        
        if (daySchedule == null)
            return false;
            
        if (daySchedule.IsClosed)
            return false;
            
        var currentTime = dateTime.TimeOfDay;
        
        return daySchedule.Intervals.Any(interval => 
            currentTime >= interval.OpenTime && 
            currentTime <= interval.CloseTime);
    }
    
    public void UpdateDetails(string name, string? description, PriceRange priceRange)
    {
        Name = name;
        Description = description;
        PriceRange = priceRange;
    }

    public void SetLocation(string address, decimal latitude, decimal longitude)
    {
        Location = new Location(address, latitude, longitude);
    }

    public void SetContact(ShopContactDto contact)
    {
        Contact = ShopContact.Create(contact.InstagramLink, contact.Email, contact.SiteLink,
            contact.PhoneNumber);
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