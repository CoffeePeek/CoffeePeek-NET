using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Microsoft.EntityFrameworkCore;
using OutboxEvent = CoffeePeek.Shops.Domain.Entities.OutboxEvent;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Infrastructure.Configuration;

public class ShopsDbContext(DbContextOptions<ShopsDbContext> options) : DbContext(options)
{
    public virtual DbSet<BrewMethod> BrewMethods { get; set; }
    
    public virtual DbSet<CoffeeBean> CoffeeBeans { get; set; }
    
    public virtual DbSet<Equipment> Equipments { get; set; }
    
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<CheckIn> CheckIns { get; set; }
    
    public virtual DbSet<Roaster> Roasters { get; set; }
    
    public virtual DbSet<UserFavorite> UserFavorites { get; set; }
    public virtual DbSet<UserVisit> UserVisits { get; set; }
    public virtual DbSet<CoffeeShop> Shops { get; set; }
    
    public virtual DbSet<ShopPhoto> ShopPhotos { get; set; }
    
    public virtual DbSet<City> Cities { get; set; }
    
    public virtual DbSet<OutboxEvent> OutboxEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CoffeeShopConfiguration());

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
        
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.ShopId);
            entity.HasIndex(c => new { c.UserId, c.ShopId });
            
            entity.HasOne(c => c.CoffeeShop)
                .WithMany(s => s.CheckIns)
                .HasForeignKey(c => c.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(c => c.Review)
                .WithMany()
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.Property(c => c.Note).HasMaxLength(BusinessConstants.MaxCheckInNoteLength);
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

            entity.HasIndex(v => new { v.UserId, v.ShopId }).IsUnique();

            entity.HasIndex(v => v.UserId);

            entity.HasIndex(v => new { v.UserId, v.LastVisitedAt });

            entity.HasIndex(v => new { v.UserId, v.VisitCount });
            
            entity.Property(v => v.UserId).IsRequired();
            entity.Property(v => v.ShopId).IsRequired();
            entity.Property(v => v.FirstVisitedAt).IsRequired();
            entity.Property(v => v.LastVisitedAt).IsRequired();
            entity.Property(v => v.VisitCount).IsRequired().HasDefaultValue(1);
            entity.Property(v => v.HasReview).IsRequired().HasDefaultValue(false);
            entity.Property(v => v.Note).HasMaxLength(BusinessConstants.MaxVisitNoteLength);
        });
    }
}