namespace CoffeePeek.Contract.Dtos.Address;

public class AddressDto
{
    public int CityId { get; set; }
    public int StreetId { get; set; }
    public string CityName { get; set; } 
    public string StreetName { get; set; }
    public string BuildingNumber { get; set; }
    public string PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}