namespace CoffeePeek.ShopsService.Entities;

public class FavoriteShop
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }

    public ICollection<Shop> Shops { get; set; } = new HashSet<Shop>();
}