using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.ShopTagAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Shops.Persistance.Configuration;

public class ShopTagConfiguration : IEntityTypeConfiguration<ShopTag>
{
    public void Configure(EntityTypeBuilder<ShopTag> builder)
    {
        builder.ToTable("ShopTags");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxShopTagSlugLength);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(BusinessConstants.MaxShopTagNameLength);

        builder.Property(t => t.Description)
            .HasMaxLength(BusinessConstants.MaxShopTagDescriptionLength);

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.SortOrder);
    }
}
