namespace CoffeePeek.Domain.Entities.Address;

public class Address : BaseEntity
{
    public int CityId { get; set; }
    public int StreetId { get; set; }
    public string BuildingNumber { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public City City { get; set; }
    public Street Street { get; set; }
}