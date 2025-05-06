using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Photos;
using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class CoffeeShopDto 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public AddressDto Address { get; set; }
    public ShopContactDto ShopContact { get; set; }
    public List<ShopPhotoDtos> ShopPhotos { get; set; }
    public List<ScheduleDto> Schedules { get; set; }
}