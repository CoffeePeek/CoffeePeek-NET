using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed class ModerationLocation : Entity<Guid>
{
    public string Address { get; private set; }
    public bool IsAddressValidated { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    public Guid ShopId { get; private set; }
    public ModerationShop? ModerationShop { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ModerationLocation()
    {
    }

    public ModerationLocation(Guid shopId, string address)
    {
        ShopId = shopId;
        Address = address;
        IsAddressValidated = false;
    }
    
    public ModerationLocation(Guid shopId, string validatedAddress, decimal lat, decimal lon)
    {
        Id = Guid.NewGuid();
        ShopId = shopId;
        Latitude = lat;
        Longitude = lon;
        Address = validatedAddress;
        IsAddressValidated = true;
    }
    
    public void SetLocation(decimal lat, decimal lon, string validatedAddress)
    {
        Latitude = lat;
        Longitude = lon;
        Address = validatedAddress;
        IsAddressValidated = true;
    }
}