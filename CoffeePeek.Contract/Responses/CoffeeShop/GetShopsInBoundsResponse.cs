using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Responses.CoffeeShop;

public class GetShopsInBoundsResponse
{
    /// <summary>
    /// Инициализирует ответ, содержащий коллекцию кофеен, попадающих в заданные границы.
    /// </summary>
    /// <param name="shops">Перечисление DTO кофеен; будет материализовано в массив и присвоено свойству <c>Shops</c>.</param>
    public GetShopsInBoundsResponse(IEnumerable<MapShopDto> shops)
    {
        Shops = shops.ToArray();
    }

    public MapShopDto[] Shops { get; set; }
}
