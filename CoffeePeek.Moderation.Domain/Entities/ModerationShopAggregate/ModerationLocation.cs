namespace CoffeePeek.Moderation.Domain.Entities;

public record ModerationLocation
{
    public string Address { get; private set; }
    public bool IsAddressValidated { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }


    // ReSharper disable once UnusedMember.Local
    private ModerationLocation()
    {
    }
    
    public ModerationLocation(string validatedAddress, decimal lat, decimal lon)
    {
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