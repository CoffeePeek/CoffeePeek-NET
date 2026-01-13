using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public class GetCoffeeShopsResponse
{
    public List<ShortShopDto> CoffeeShops { get; set; }

    [JsonIgnore]
    public int CurrentPage { get; set; }
    [JsonIgnore]
    public int PageSize { get; set; }
    [JsonIgnore]
    public int TotalItems { get; set; }
    [JsonIgnore]
    public int TotalPages { get; set; }
}