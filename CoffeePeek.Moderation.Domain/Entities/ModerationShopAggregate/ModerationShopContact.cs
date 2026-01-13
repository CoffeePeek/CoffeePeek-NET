namespace CoffeePeek.Moderation.Domain.Entities;

public record ModerationShopContact
{
    public string? PhoneNumber { get; private set; }
    public string? InstagramLink { get; private set; }
    public string? Email { get; private set; }
    public string? SiteLink { get; private set; }
    
    private ModerationShopContact() { }
    
    public static ModerationShopContact Create(string? phoneNumber, string? instagramLink,
        string? email, string? siteLink)
    {
        return new ModerationShopContact
        {
            PhoneNumber = phoneNumber,
            InstagramLink = instagramLink,
            Email = email,
            SiteLink = siteLink,
        };
    }
}