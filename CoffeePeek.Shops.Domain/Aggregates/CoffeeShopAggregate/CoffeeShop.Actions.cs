using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.CoffeeShopAggregate.Enums;
using CoffeePeek.Shops.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public partial class CoffeeShop
{
    public bool IsNew => CreatedAtUtc > DateTime.UtcNow.AddDays(-BusinessConstants.ItNewEntityInDays);
    public bool IsOpen => IsOpenAt(DateTime.UtcNow);
    
    public decimal Rating => Reviews.Count == 0 ? 0 : Reviews.Average(r => r.Rating.AverageRating);

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
}