namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public record Location
{
    public string Address { get; private set; }
    public bool IsAddressValidated { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private Location() { }
    
    public Location(string validatedAddress, decimal lat, decimal lon)
    {
        Latitude = lat;
        Longitude = lon;
        Address = validatedAddress;
        IsAddressValidated = true;
    }
}