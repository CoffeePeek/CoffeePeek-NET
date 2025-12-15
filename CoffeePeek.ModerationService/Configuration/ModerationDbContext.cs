using CoffeePeek.ModerationService.Entities;
using CoffeePeek.ModerationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ModerationService.Configuration;

public class ModerationDbContext(DbContextOptions<ModerationDbContext> options) : DbContext(options)
{
    public DbSet<ModerationShop> ModerationShops { get; set; }
    public DbSet<ShopContacts> ShopContacts { get; set; }
    public DbSet<ShopPhoto> ShopPhotos { get; set; }
    public DbSet<ModerationShopSchedule> ModerationShopSchedules { get; set; }
    public DbSet<ModerationShopScheduleInterval> ModerationShopScheduleIntervals { get; set; }
    public DbSet<Location> ModerationLocations { get; set; }
    public DbSet<ModerationShopEquipment> ModerationShopEquipments { get; set; }
    public DbSet<ModerationCoffeeBeanShop> ModerationCoffeeBeanShops { get; set; }
    public DbSet<ModerationRoasterShop> ModerationRoasterShops { get; set; }
    public DbSet<ModerationShopBrewMethod> ModerationShopBrewMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ModerationShop>(entity =>
        {
            entity.ToTable("ModerationShops");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NotValidatedAddress).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ModerationStatus);
            entity.HasIndex(e => e.CityId);
            
            entity.HasOne(e => e.ShopContacts)
                .WithMany()
                .HasForeignKey(e => e.ShopContactId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Location)
                .WithOne(l => l.ModerationShop)
                .HasForeignKey<Location>(l => l.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.ShopPhotos)
                .WithOne()
                .HasForeignKey("ShopId")
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
        });
        
        
        modelBuilder.Entity<ShopContacts>(entity =>
        {
            entity.ToTable("ShopContacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhoneNumber).HasMaxLength(18);
            entity.Property(e => e.InstagramLink).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.SiteLink).HasMaxLength(200);
        });
        
        modelBuilder.Entity<ShopPhoto>(entity =>
        {
            entity.ToTable("ShopPhotos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(500);
        });
        
        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("ModerationLocations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).HasMaxLength(200);
        });
        
        modelBuilder.Entity<ModerationShopEquipment>(entity =>
        {
            entity.ToTable("ModerationShopEquipments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.EquipmentId);
        });
        
        modelBuilder.Entity<ModerationCoffeeBeanShop>(entity =>
        {
            entity.ToTable("ModerationCoffeeBeanShops");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.CoffeeBeanId);
        });
        
        modelBuilder.Entity<ModerationRoasterShop>(entity =>
        {
            entity.ToTable("ModerationRoasterShops");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.RoasterId);
        });
        
        modelBuilder.Entity<ModerationShopBrewMethod>(entity =>
        {
            entity.ToTable("ModerationShopBrewMethods");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.BrewMethodId);
        });
    }
}

