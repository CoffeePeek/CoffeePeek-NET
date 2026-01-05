using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed class ModerationShopContact : Entity<Guid>
{
    public DateTime CreatedAt { get; private set; }
    public Guid ModerationShopId { get; private set; }
    [MaxLength(18)]
    public string? PhoneNumber { get; private set; }
    [MaxLength(50)]
    public string? InstagramLink { get; private set; }
    public string? Email { get; private set; }
    public string? SiteLink { get; private set; }
    
    public ModerationShop? ModerationShop { get; private set; }
    private ModerationShopContact() { }
    
    public void Update(string? phone, string? email, string? instagram, string? site)
    {
        if (string.IsNullOrEmpty(phone))
        {
            PhoneNumber = phone;
        }
        
        if (string.IsNullOrEmpty(email))
        {
            Email = email;
        }
        
        if (string.IsNullOrEmpty(instagram))
        {
            InstagramLink = instagram;
        }
        
        if (string.IsNullOrEmpty(site))
        {
            SiteLink = site;
        }
    }
    
    public static ModerationShopContact Create(Guid moderationShopId, string? phoneNumber, string? instagramLink,
        string? email, string? siteLink)
    {
        return new ModerationShopContact
        {
            Id = Guid.NewGuid(),
            ModerationShopId = moderationShopId,
            CreatedAt = DateTime.UtcNow,
            PhoneNumber = phoneNumber,
            InstagramLink = instagramLink,
            Email = email,
            SiteLink = siteLink,
        };
    }
}

