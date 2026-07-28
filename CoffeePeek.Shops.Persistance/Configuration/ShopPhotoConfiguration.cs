using CoffeePeek.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Shops.Persistance.Configuration;

public class ShopPhotoConfiguration : IEntityTypeConfiguration<ShopPhoto>
{
    public void Configure(EntityTypeBuilder<ShopPhoto> builder)
    {
        builder.Property(p => p.SortIndex)
            .IsRequired()
            .HasDefaultValue(0);

        // CoffeeShopId is a shadow FK on ShopPhotos
        builder.HasIndex("CoffeeShopId", nameof(ShopPhoto.SortIndex))
            .HasDatabaseName("IX_ShopPhotos_CoffeeShopId_SortIndex");
    }
}
