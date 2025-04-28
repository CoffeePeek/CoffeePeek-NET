using CoffeePeek.Domain.Entities.Schedules;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.Enums.Shop;

namespace CoffeePeek.Domain.Entities.Shop;

public class ReviewShop : BaseEntity
{
    public ReviewShop()
    {
        ShopPhotos = new HashSet<ShopPhoto>();
        Schedules = new HashSet<Schedule>();
        ScheduleExceptions = new HashSet<ScheduleException>();
    }
    
    public string Name { get; set; }
    public string NotValidatedAddress { get; set; }
    public int UserId { get; set; }
    public int? AddressId { get; set; }
    public int? ShopContactId { get; set; }
    public int? ShopId { get; set; }

    public ReviewStatus ReviewStatus { get; set; }
    public Address.Address? Address { get; set; }
    public ShopStatus Status { get; set; }
    public virtual ShopContacts? ShopContacts { get; set; }
    public virtual User User { get; set; }
    public virtual Shop? Shop { get; set; }
    public ICollection<ShopPhoto>? ShopPhotos { get; set; }
    public ICollection<Schedule>? Schedules { get; set; }
    public ICollection<ScheduleException>? ScheduleExceptions { get; set; }
}