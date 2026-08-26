using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Shops.Persistance.Configuration;

public class CoffeeDrinkDefinitionConfiguration : IEntityTypeConfiguration<CoffeeDrinkDefinition>
{
    public void Configure(EntityTypeBuilder<CoffeeDrinkDefinition> builder)
    {
        builder.ToTable("CoffeeDrinkDefinitions");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Slug)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxCoffeeDrinkSlugLength);

        builder.Property(d => d.NameRu)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxCoffeeDrinkNameLength);

        builder.Property(d => d.NameEn)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxCoffeeDrinkNameLength);

        builder.Property(d => d.Aliases)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxCoffeeDrinkAliasesLength);

        builder.HasIndex(d => d.Slug).IsUnique();
        builder.HasIndex(d => d.SortOrder);
    }
}

public class ShopMenuConfiguration : IEntityTypeConfiguration<ShopMenu>
{
    public void Configure(EntityTypeBuilder<ShopMenu> builder)
    {
        builder.ToTable("ShopMenus");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Currency)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxMenuCurrencyLength);

        builder.Property(m => m.ParseError)
            .HasMaxLength(BusinessConstants.MaxMenuParseErrorLength);

        builder.HasIndex(m => m.CoffeeShopId).IsUnique();

        builder.HasOne<CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShop>()
            .WithMany()
            .HasForeignKey(m => m.CoffeeShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Items)
            .WithOne()
            .HasForeignKey(i => i.ShopMenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Photos)
            .WithOne()
            .HasForeignKey(p => p.ShopMenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(m => m.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ShopMenuItemConfiguration : IEntityTypeConfiguration<ShopMenuItem>
{
    public void Configure(EntityTypeBuilder<ShopMenuItem> builder)
    {
        builder.ToTable("ShopMenuItems");
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => new { i.ShopMenuId, i.DrinkDefinitionId }).IsUnique();
        builder.Property(i => i.Price).HasPrecision(10, 2);
        builder.Property(i => i.CustomName).HasMaxLength(BusinessConstants.MaxCoffeeDrinkNameLength);

        builder.HasOne(i => i.DrinkDefinition)
            .WithMany()
            .HasForeignKey(i => i.DrinkDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ShopMenuPhotoConfiguration : IEntityTypeConfiguration<ShopMenuPhoto>
{
    public void Configure(EntityTypeBuilder<ShopMenuPhoto> builder)
    {
        builder.ToTable("ShopMenuPhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxMenuPhotoFileNameLength);

        builder.Property(p => p.ContentType)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxMenuPhotoContentTypeLength);

        builder.Property(p => p.StorageKey)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxMenuPhotoStorageKeyLength);
    }
}
