using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Moderation.Domain.Entities;

public class ModerationShop
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string Name { get; set; }
    public string? Description { get; set; }
    [MaxLength(150)]
    public string NotValidatedAddress { get; set; }
    public string Address { get; set; }
    public Guid UserId { get; set; }
    public Guid? ShopContactId { get; set; }
    public Guid? ShopId { get; set; }

    public PriceRange PriceRange { get; set; }
    public Guid CityId { get; set; }
    public Guid? LocationId { get; set; }

    public ModerationStatus ModerationStatus { get; set; }
    public ShopStatus Status { get; set; }
    
    public bool IsAddressValidated { get; set; } = false;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public virtual ShopContacts? ShopContacts { get; set; }
    public virtual Location? Location { get; set; }
    public ICollection<ShopPhoto> ShopPhotos { get; set; } = new HashSet<ShopPhoto>();
    public ICollection<ModerationShopSchedule> Schedules { get; set; } = new HashSet<ModerationShopSchedule>();
    public ICollection<ModerationShopEquipment> ModerationShopEquipments { get; set; } = new HashSet<ModerationShopEquipment>();
    public ICollection<ModerationCoffeeBeanShop> ModerationCoffeeBeanShops { get; set; } = new HashSet<ModerationCoffeeBeanShop>();
    public ICollection<ModerationRoasterShop> ModerationRoasterShops { get; set; } = new HashSet<ModerationRoasterShop>();
    public ICollection<ModerationShopBrewMethod> ModerationShopBrewMethods { get; set; } = new HashSet<ModerationShopBrewMethod>();
}
