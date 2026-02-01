using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Moderation.Infrastructure.Configuration;

public class ModerationShopConfiguration : IEntityTypeConfiguration<ModerationShop>
{
    public void Configure(EntityTypeBuilder<ModerationShop> entity)
    {
        entity.UsePropertyAccessMode(PropertyAccessMode.Field);

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).HasMaxLength(50);
        entity.Property(e => e.Description).HasMaxLength(1000);
        entity.Property(e => e.RejectedReason).HasMaxLength(200);
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.ModerationStatus);
        entity.HasIndex(e => e.CityId);

        entity.OwnsOne(e => e.Contact, contact =>
        {
            contact.Property(sc => sc.PhoneNumber).HasMaxLength(BusinessConstants.MaxShopContactPhoneNumberLength);
            contact.Property(sc => sc.Email).HasMaxLength(BusinessConstants.MaxShopContactEmailLength);
            contact.Property(sc => sc.SiteLink).HasMaxLength(BusinessConstants.MaxShopContactSiteLinkLength);
            contact.Property(sc => sc.InstagramLink)
                .HasMaxLength(BusinessConstants.MaxShopContactInstagramLinkLength);
        });

        entity.OwnsOne(e => e.Location, location => { location.HasIndex(l => new { l.Latitude, l.Longitude }); });

        entity.OwnsMany(e => e.Schedules, schedules =>
        {
            schedules.OwnsMany(sc => sc.Intervals, intervals =>
            {
                intervals.Property(i => i.OpenTime).IsRequired();
                intervals.Property(i => i.CloseTime).IsRequired();
            });
        });

        entity.HasMany(e => e.ShopPhotos)
            .WithOne()
            .HasForeignKey(x => x.ModerationShopId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.ModerationShopEquipments)
            .WithOne(eq => eq.ModerationShop)
            .HasForeignKey(eq => eq.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.ModerationCoffeeBeanShops)
            .WithOne(cb => cb.ModerationShop)
            .HasForeignKey(cb => cb.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.ModerationRoasterShops)
            .WithOne(rs => rs.ModerationShop)
            .HasForeignKey(rs => rs.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.ModerationShopBrewMethods)
            .WithOne(bm => bm.ModerationShop)
            .HasForeignKey(bm => bm.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}