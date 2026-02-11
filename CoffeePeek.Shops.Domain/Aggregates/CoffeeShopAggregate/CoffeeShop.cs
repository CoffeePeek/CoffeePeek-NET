using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

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

    public void SetContact(string? instagramLink, string? email, string? siteLink, string? phoneNumber)
    {
        Contact = ShopContact.Create(instagramLink, email, siteLink, phoneNumber);
    }
    
    public void AddPhotos(IEnumerable<ShopPhoto> photos)
    {
        _shopPhotos.AddRange(photos);
    }
    
    public void AddEquipment(Equipment equipment)
    {
        if (_equipments.Any(e => e.Brand == equipment.Brand && e.ModelName == equipment.ModelName))
            return;
        
        if (equipment.IsPrimary)
        {
            foreach (var e in _equipments.Where(e => e.CategoryId == equipment.CategoryId))
            {
                e.UnmarkAsPrimary();
            }
        }

        _equipments.Add(equipment);
    }

    public void RemoveEquipment(Guid equipmentId)
    {
        var equipment = _equipments.FirstOrDefault(e => e.Id == equipmentId);
        if (equipment != null)
        {
            _equipments.Remove(equipment);
        }
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
    
    public void AddSchedule(List<ShopSchedule> schedule)
    {
        _schedules.AddRange(schedule);
    }
    
    #endregion
}