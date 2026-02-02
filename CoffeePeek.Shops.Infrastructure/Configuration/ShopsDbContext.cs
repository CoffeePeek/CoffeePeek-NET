using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.CheckInAggregate;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Microsoft.EntityFrameworkCore;
using Review = CoffeePeek.Shops.Domain.Entities.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Infrastructure.Configuration;

public class ShopsDbContext(DbContextOptions<ShopsDbContext> options) : DbContext(options)
{
    //category
    public virtual DbSet<BrewMethod> BrewMethods { get; set; }
    
    public virtual DbSet<CoffeeBean> CoffeeBeans { get; set; }
    
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<CheckIn> CheckIns { get; set; }
    
    public virtual DbSet<Roaster> Roasters { get; set; }
    
    public virtual DbSet<UserFavorite> UserFavorites { get; set; }
    public virtual DbSet<CoffeeShop> Shops { get; set; }
    
    public virtual DbSet<ShopPhoto> ShopPhotos { get; set; }
    
    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<EquipmentCategory> EquipmentCategories { get; set; }
    public virtual DbSet<Equipment> Equipments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CoffeeShopConfiguration());

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Shop)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.CoffeeShopId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(r => r.UserId);

            entity.Property(r => r.Header).HasMaxLength(BusinessConstants.MaxReviewHeaderLength);
            entity.Property(r => r.Comment).HasMaxLength(BusinessConstants.MaxReviewCommentLength);
            entity.Property(r => r.UserName).IsRequired().HasMaxLength(30);

            entity.OwnsOne<Rating>(r => r.Rating);
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
            
            entity.OwnsOne<Rating>(r => r.Rating);
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.CoffeeShopId }).IsUnique();

            entity.HasIndex(f => f.UserId);

            entity.HasIndex(f => f.CoffeeShopId);

            entity.Property(f => f.UserId).IsRequired();
            entity.Property(f => f.CoffeeShopId).IsRequired();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CategoryId);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<EquipmentCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(BusinessConstants.MaxEquipmentCategoryNameLength);
        });
    }
}