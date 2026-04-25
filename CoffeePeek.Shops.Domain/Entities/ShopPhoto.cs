using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Entities;

public sealed class ShopPhoto : Entity<Guid>
{
    [MaxLength(50)] public string FileName { get; private set; }
    [MaxLength(30)] public string ContentType { get; private set; }
    [MaxLength(200)] public string StorageKey { get; private set; }

    public long SizeBytes { get; private set; }
    public Guid OwnerId { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ShopPhoto() { }

    public ShopPhoto(string fileName, string contentType, string storageKey, long sizeBytes, Guid ownerId)
    {
        FileName = fileName;
        ContentType = contentType;
        StorageKey = storageKey;
        SizeBytes = sizeBytes;
        OwnerId = ownerId;
    }
}