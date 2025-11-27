namespace CoffeePeek.Web.Contract.Http.Shops;

public class CoffeeShopDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public AddressDto Address { get; set; }
    public ShopContactDto? ShopContact { get; set; }
    public List<ShopPhotoDtos> ShopPhotos { get; set; }
    public List<ScheduleDto> Schedules { get; set; }
}