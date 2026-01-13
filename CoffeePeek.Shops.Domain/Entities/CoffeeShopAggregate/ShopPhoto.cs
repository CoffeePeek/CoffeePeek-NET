using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public sealed class ShopPhoto : Entity<Guid>
{
    [MaxLength(50)]
    public string FileName { get; private set; }
    [MaxLength(30)]
    public string ContentType { get; private set; }
    [MaxLength(200)]
    public string StorageKey { get; private set; } = null!;
    
    public long SizeBytes { get; private set; }
    public Guid OwnerId { get; private set; }
    
    public Guid ShopId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public CoffeeShop CoffeeShop { get; private set; } = null!;
    
    // ReSharper disable once UnusedMember.Local
    private ShopPhoto() { }

    public ShopPhoto(string fileName, string contentType, string storageKey, long sizeBytes, Guid ownerId, Guid shopId)
    {
        FileName = fileName;
        ContentType = contentType;
        StorageKey = storageKey;
        SizeBytes = sizeBytes;
        OwnerId = ownerId;
        ShopId = shopId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}