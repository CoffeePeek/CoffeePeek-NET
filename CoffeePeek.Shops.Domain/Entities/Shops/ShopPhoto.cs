namespace CoffeePeek.Shops.Domain.Entities;

public class ShopPhoto : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public string Url { get; private set; } = null!;

    public virtual Shop Shop { get; private set; } = null!;
    
    // ReSharper disable once UnusedMember.Local
    private ShopPhoto() { }

    public ShopPhoto(Guid id, Guid userId, string url)
    {
        Id = id;
        UserId = userId;
        Url = url;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    #region Domain Methods

    internal void SetShop(Guid shopId)
    {
        if (shopId == Guid.Empty)
            throw new ArgumentException("ShopId cannot be empty", nameof(shopId));

        ShopId = shopId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("URL cannot be empty", nameof(newUrl));

        Url = newUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}