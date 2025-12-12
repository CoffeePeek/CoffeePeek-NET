using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.ModerationService.Models;

public class ModerationShop
{
    public ModerationShop()
    {
        ShopPhotos = new HashSet<ShopPhoto>();
        Schedules = new HashSet<Schedule>();
        ScheduleExceptions = new HashSet<ScheduleException>();
    }
    
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(150)]
    public string NotValidatedAddress { get; set; }
    public string Address { get; set; }
    public Guid UserId { get; set; }
    public Guid? ShopContactId { get; set; }
    public Guid? ShopId { get; set; }

    public ModerationStatus ModerationStatus { get; set; }
    public ShopStatus Status { get; set; }
    
    public bool IsAddressValidated { get; set; } = false;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public virtual ShopContacts? ShopContacts { get; set; }
    public ICollection<ShopPhoto> ShopPhotos { get; set; }
    public ICollection<Schedule> Schedules { get; set; }
    public ICollection<ScheduleException> ScheduleExceptions { get; set; }
}
