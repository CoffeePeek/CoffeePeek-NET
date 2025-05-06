using CoffeePeek.Domain.Entities.Schedules;
using CoffeePeek.Domain.Enums.Shop;

namespace CoffeePeek.Domain.Entities.Shop;

public class Shop : BaseEntity
{
    public Shop()
    {
        ShopPhotos = new HashSet<ShopPhoto>();
        Schedules = new HashSet<Schedule>();
        ScheduleExceptions = new HashSet<ScheduleException>();
        Reviews = new HashSet<Review.Review>();
    }
    
    public string Name { get; set; }
    public int AddressId { get; set; }
    public int? ShopContactId { get; set; }

    public Address.Address Address { get; set; }
    public ShopStatus Status { get; set; }
    public virtual ShopContacts ShopContacts { get; set; }

    public ICollection<ShopPhoto> ShopPhotos { get; set; }
    public ICollection<Schedule> Schedules { get; set; }
    public ICollection<ScheduleException> ScheduleExceptions { get; set; }
    public ICollection<Review.Review> Reviews { get; set; }
}