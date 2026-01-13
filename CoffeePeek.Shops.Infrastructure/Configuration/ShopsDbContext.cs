using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.City;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Microsoft.EntityFrameworkCore;
using OutboxEvent = CoffeePeek.Shops.Domain.Entities.OutboxEvent;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Infrastructure.Configuration;

public class ShopsDbContext(DbContextOptions<ShopsDbContext> options) : DbContext(options)
{
    public virtual DbSet<BrewMethod> BrewMethods { get; set; }
    public virtual DbSet<ShopBrewMethod> ShopBrewMethods { get; set; }
    
    public virtual DbSet<CoffeeBean> CoffeeBeans { get; set; }
    public virtual DbSet<CoffeeBeanShop> CoffeeBeanShops { get; set; }
    
    public virtual DbSet<Equipment> Equipments { get; set; }
    public virtual DbSet<ShopEquipment> ShopEquipments { get; set; }
    
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<CheckIn> CheckIns { get; set; }
    
    public virtual DbSet<Roaster> Roasters { get; set; }
    public virtual DbSet<RoasterShop> RoasterShops { get; set; }
    
    public virtual DbSet<UserFavorite> UserFavorites { get; set; }
    public virtual DbSet<UserVisit> UserVisits { get; set; }
    public virtual DbSet<CoffeeShop> Shops { get; set; }
    
    public virtual DbSet<ShopPhoto> ShopPhotos { get; set; }
    
    public virtual DbSet<City> Cities { get; set; }
    
    public virtual DbSet<OutboxEvent> OutboxEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Shop)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(r => r.UserId);

            entity.Property(r => r.Header).HasMaxLength(BusinessConstants.MaxReviewHeaderLength);
            entity.Property(r => r.Comment).HasMaxLength(BusinessConstants.MaxReviewCommentLength);
        });

        modelBuilder.Entity<CoffeeShop>(entity =>
        {
            entity.HasKey(s => s.Id);
    
            var navigationPhotos = entity.Metadata.FindNavigation(nameof(CoffeeShop.ShopPhotos));
            navigationPhotos?.SetPropertyAccessMode(PropertyAccessMode.Field);

            var navigationSchedules = entity.Metadata.FindNavigation(nameof(CoffeeShop.Schedules));
            navigationSchedules?.SetPropertyAccessMode(PropertyAccessMode.Field);
            
            entity.OwnsOne(e => e.Contact, contact =>
            {
                contact.Property(sc => sc.PhoneNumber).HasMaxLength(BusinessConstants.MaxShopContactPhoneNumberLength);
                contact.Property(sc => sc.Email).HasMaxLength(BusinessConstants.MaxShopContactEmailLength);
                contact.Property(sc => sc.SiteLink).HasMaxLength(BusinessConstants.MaxShopContactSiteLinkLength);
                contact.Property(sc => sc.InstagramLink).HasMaxLength(BusinessConstants.MaxShopContactInstagramLinkLength);
            });
            
            entity.OwnsOne(e => e.Location, location =>
            {
                location.HasIndex(l => new { l.Latitude, l.Longitude });
            });

            entity.OwnsMany(e => e.Schedules, schedules =>
            {
                schedules.OwnsMany(sc => sc.Intervals, intervals =>
                    {
                        intervals.Property(i => i.OpenTime).IsRequired();
                        intervals.Property(i => i.CloseTime).IsRequired();
                    });
            });
        });
        
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.ShopId);
            entity.HasIndex(c => new { c.UserId, c.ShopId, c.CreatedAt });
            
            entity.HasOne(c => c.CoffeeShop)
                .WithMany(s => s.CheckIns)
                .HasForeignKey(c => c.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(c => c.Review)
                .WithMany()
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.Property(c => c.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.CoffeeShopId }).IsUnique();

            entity.HasIndex(f => f.UserId);

            entity.HasIndex(f => f.CoffeeShopId);

            entity.Property(f => f.UserId).IsRequired();
            entity.Property(f => f.CoffeeShopId).IsRequired();
        });

        modelBuilder.Entity<UserVisit>(entity =>
        {
            entity.HasKey(v => v.Id);
            
            // Уникальный индекс: одна запись на пару User-Shop
            entity.HasIndex(v => new { v.UserId, v.ShopId })
                .IsUnique()
                .HasDatabaseName("IX_UserVisit_User_Shop_Unique");
            
            // Индекс для поиска посещений пользователя
            entity.HasIndex(v => v.UserId)
                .HasDatabaseName("IX_UserVisit_UserId");
            
            // Индекс для сортировки по последнему посещению
            entity.HasIndex(v => new { v.UserId, v.LastVisitedAt })
                .HasDatabaseName("IX_UserVisit_UserId_LastVisited");
            
            // Индекс для поиска самых популярных мест
            entity.HasIndex(v => new { v.UserId, v.VisitCount })
                .HasDatabaseName("IX_UserVisit_UserId_VisitCount");
            
            // Свойства
            entity.Property(v => v.UserId).IsRequired();
            entity.Property(v => v.ShopId).IsRequired();
            entity.Property(v => v.FirstVisitedAt).IsRequired();
            entity.Property(v => v.LastVisitedAt).IsRequired();
            entity.Property(v => v.VisitCount).IsRequired().HasDefaultValue(1);
            entity.Property(v => v.HasReview).IsRequired().HasDefaultValue(false);
            entity.Property(v => v.Note).HasMaxLength(500);
        });
    }
}