
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Domain.Entities;

public class Shop : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PriceRange PriceRange { get; private set; }
    public Guid CityId { get; private set; }

    public Guid? ShopContactId { get; private set; }
    public Guid? LocationId { get; private set; }

    public virtual ShopContact? ShopContact { get; private set; }
    public virtual Location? Location { get; private set; }

    private readonly List<ShopSchedule> _schedules = [];
    public virtual IReadOnlyCollection<ShopSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<ShopPhoto> _shopPhotos = [];
    public virtual IReadOnlyCollection<ShopPhoto> ShopPhotos => _shopPhotos.AsReadOnly();

    private readonly List<ShopEquipment> _shopEquipments = [];
    public virtual IReadOnlyCollection<ShopEquipment> ShopEquipments => _shopEquipments.AsReadOnly();

    private readonly List<CoffeeBeanShop> _coffeeBeanShops = [];
    public virtual IReadOnlyCollection<CoffeeBeanShop> CoffeeBeanShops => _coffeeBeanShops.AsReadOnly();

    private readonly List<RoasterShop> _roasterShops = [];
    public virtual IReadOnlyCollection<RoasterShop> RoasterShops => _roasterShops.AsReadOnly();

    private readonly List<ShopBrewMethod> _shopBrewMethods = [];
    public virtual IReadOnlyCollection<ShopBrewMethod> ShopBrewMethods => _shopBrewMethods.AsReadOnly();

    public virtual ICollection<Review> Reviews { get; private set; } = new HashSet<Review>();
    public virtual ICollection<CheckIn> CheckIns { get; private set; } = new HashSet<CheckIn>();

    // ReSharper disable once UnusedMember.Local
    private Shop()
    {
    }
    
    public Shop(Guid id, string name, Guid cityId, PriceRange priceRange)
    {
        Id = id;
        Name = name;
        CityId = cityId;
        PriceRange = priceRange;
    }

    #region Domain Logic

    public void UpdateDetails(string name, string? description, PriceRange priceRange)
    {
        Name = name;
        Description = description;
        PriceRange = priceRange;
    }

    public void SetLocation(Location? location)
    {
        if (location == null) return;
        location.ShopId = this.Id;
        Location = location;
        LocationId = location.Id;
    }

    public void SetContact(ShopContact? contact)
    {
        if (contact == null) return;
        contact.ShopId = this.Id;
        ShopContact = contact;
        ShopContactId = contact.Id;
    }
    
    public void AddPhotos(IEnumerable<ShopPhoto> photos)
    {
        _shopPhotos.AddRange(photos);
    }
    #endregion
}