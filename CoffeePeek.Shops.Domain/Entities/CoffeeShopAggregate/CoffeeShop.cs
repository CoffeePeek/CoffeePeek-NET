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

    public void SetLocation(Guid cityId, string address, decimal latitude, decimal longitude)
    {
        Location = Location.CreateValidated(cityId, address, latitude, longitude);
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
    
    public void SetEquipment(IEnumerable<Equipment> equipments)
    {
        _equipments.Clear();
        _equipments.AddRange(equipments);
    }
    
    public void SetBrewMethods(IEnumerable<BrewMethod> methods)
    {
        _brewMethods.Clear();
        _brewMethods.AddRange(methods);
    }

    public void SetRoasters(IEnumerable<Roaster> roasters)
    {
        _roasters.Clear();
        _roasters.AddRange(roasters);
    }

    public void SetBeans(IEnumerable<CoffeeBean> beans)
    {
        _coffeeBeans.Clear();
        _coffeeBeans.AddRange(beans);
    }
    
    #endregion
}