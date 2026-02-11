using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Shops.Persistance.Configuration;

public class CoffeeShopConfiguration : IEntityTypeConfiguration<CoffeeShop>
{
    public void Configure(EntityTypeBuilder<CoffeeShop> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(x => x.Name).HasMaxLength(BusinessConstants.MaxCoffeeShopNameLength);
        builder.Property(x => x.Description).HasMaxLength(BusinessConstants.MaxCoffeeShopDescriptionLength);

        var navigationPhotos = builder.Metadata.FindNavigation(nameof(CoffeeShop.ShopPhotos));
        navigationPhotos?.SetPropertyAccessMode(PropertyAccessMode.Field);

        var navigationSchedules = builder.Metadata.FindNavigation(nameof(CoffeeShop.Schedules));
        navigationSchedules?.SetPropertyAccessMode(PropertyAccessMode.Field);

        #region Value-objects

        builder.OwnsOne(e => e.Contact, contact =>
        {
            contact
                .Property(sc => sc.PhoneNumber)
                .HasColumnName(nameof(ShopContact.PhoneNumber))
                .HasMaxLength(BusinessConstants.MaxShopContactPhoneNumberLength);
            
            contact
                .Property(sc => sc.Email)
                .HasColumnName(nameof(ShopContact.Email))
                .HasMaxLength(BusinessConstants.MaxShopContactEmailLength);
            
            contact
                .Property(sc => sc.SiteLink)
                .HasColumnName(nameof(ShopContact.SiteLink))
                .HasMaxLength(BusinessConstants.MaxShopContactSiteLinkLength);
            
            contact
                .Property(sc => sc.InstagramLink)
                .HasColumnName(nameof(ShopContact.InstagramLink))
                .HasMaxLength(BusinessConstants.MaxShopContactInstagramLinkLength);
        });

        builder.OwnsOne(e => e.Location, location =>
        {
            location.HasIndex(l => new { l.Latitude, l.Longitude });

            location
                .Property(l => l.Address)
                .HasColumnName(nameof(Location.Address))
                .HasMaxLength(BusinessConstants.MaxLocationAddressLength);
            location
                .Property(l => l.Latitude)
                .HasColumnName(nameof(Location.Latitude))
                .HasPrecision(BusinessConstants.MaxLocationPrecision, BusinessConstants.MaxLocationScale);
            location
                .Property(l => l.Longitude)
                .HasColumnName(nameof(Location.Longitude))
                .HasPrecision(BusinessConstants.MaxLocationPrecision, BusinessConstants.MaxLocationScale);
            location
                .Property(l => l.IsAddressValidated)
                .HasColumnName(nameof(Location.IsAddressValidated));
            location
                .Property(l => l.CityId)
                .HasColumnName(nameof(Location.CityId));
        });

        builder.OwnsMany(e => e.Schedules, schedules =>
        {
            schedules.Property(s => s.DayOfWeek).HasColumnName(nameof(ShopSchedule.DayOfWeek));
            schedules.Property(s => s.IsClosed).HasColumnName(nameof(ShopSchedule.IsClosed));
            
            schedules.OwnsMany(sc => sc.Intervals, intervals =>
            {
                intervals.Property(i => i.OpenTime).HasColumnName(nameof(ShopScheduleInterval.OpenTime)).IsRequired();
                intervals.Property(i => i.CloseTime).HasColumnName(nameof(ShopScheduleInterval.CloseTime)).IsRequired();
            });
        });

        #endregion

        #region many-to-many

        builder
            .HasMany(u => u.BrewMethods)
            .WithMany(r => r.CoffeeShops)
            .UsingEntity<Dictionary<string, object>>(
                "CoffeeShopBrewMethods",
                j => j.HasOne<BrewMethod>().WithMany().HasForeignKey("BrewMethodId"),
                j => j.HasOne<CoffeeShop>().WithMany().HasForeignKey("CoffeeShopId")
                );

        builder
            .HasMany(u => u.Roasters)
            .WithMany(r => r.CoffeeShops)
            .UsingEntity<Dictionary<string, object>>(
                "CoffeeShopRoasters",
                j => j.HasOne<Roaster>().WithMany().HasForeignKey("RoasterId"),
                j => j.HasOne<CoffeeShop>().WithMany().HasForeignKey("CoffeeShopId")
                );

        builder
            .HasMany(u => u.CoffeeBeans)
            .WithMany(r => r.CoffeeShops)
            .UsingEntity<Dictionary<string, object>>(
                "CoffeeShopCoffeeBeans",
                j => j.HasOne<CoffeeBean>().WithMany().HasForeignKey("CoffeeBeanId"),
                j => j.HasOne<CoffeeShop>().WithMany().HasForeignKey("CoffeeShopId")
                );

        builder
            .HasMany(u => u.Equipments)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "CoffeeShopEquipments",
                j => j.HasOne<Equipment>().WithMany().HasForeignKey("EquipmentId"),
                j => j.HasOne<CoffeeShop>().WithMany().HasForeignKey("CoffeeShopId")
            );

        #endregion
    }
}