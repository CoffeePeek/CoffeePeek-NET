namespace CoffeePeek.Web.Contract.Http.Shops;

public class AddressDto
{
    public string CityName { get; set; } 
    public string StreetName { get; set; }
    public string BuildingNumber { get; set; }
    public string PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}