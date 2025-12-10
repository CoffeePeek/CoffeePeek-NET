using CoffeePeek.Contract.Enums;

namespace CoffeePeek.ShopsService.Entities;

public class Shop
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public PriceRange PriceRange { get; set; }
    
    public Guid CityId { get; set; }
    public Guid? ShopContactId { get; set; }
    public Guid? LocationId { get; set; }
    
    public virtual ShopContact? ShopContact { get; set; }
    public virtual City? City { get; set; }
    public virtual Location? Location { get; set; }
    
    public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    public virtual ICollection<CheckIn.CheckIn> CheckIns { get; set; } = new HashSet<CheckIn.CheckIn>();
    public virtual ICollection<CoffeeBeanShop> CoffeeBeanShops { get; set; } = new HashSet<CoffeeBeanShop>();
    public virtual ICollection<RoasterShop> RoasterShops { get; set; } = new HashSet<RoasterShop>();
    public virtual ICollection<ShopBrewMethod> ShopBrewMethods { get; set; } = new HashSet<ShopBrewMethod>();
    public virtual ICollection<ShopEquipment> ShopEquipments { get; set; } = new HashSet<ShopEquipment>();
    public virtual ICollection<ShopSchedule> Schedules { get; set; } = new HashSet<ShopSchedule>();
    public virtual ICollection<ShopPhoto> ShopPhotos { get; set; } = new HashSet<ShopPhoto>();
}