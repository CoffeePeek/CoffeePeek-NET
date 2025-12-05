using CoffeePeek.ShopsService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.DB;

public class ShopsDbContext(DbContextOptions<ShopsDbContext> options) : DbContext(options)
{
    public virtual DbSet<BrewMethod> BrewMethods { get; set; }
    public virtual DbSet<ShopBrewMethod> ShopBrewMethods { get; set; }
    
    public virtual DbSet<CoffeeBean> CoffeeBeans { get; set; }
    public virtual DbSet<CoffeeBeanShop> CoffeeBeanShops { get; set; }
    
    public virtual DbSet<Equipment> Equipments { get; set; }
    public virtual DbSet<ShopEquipment> ShopEquipments { get; set; }
    
    public virtual DbSet<Review> Reviews { get; set; }
    
    public virtual DbSet<Roaster> Roasters { get; set; }
    public virtual DbSet<RoasterShop> RoasterShops { get; set; }
    
    public virtual DbSet<ShopSchedule> ShopSchedules { get; set; }
    public virtual DbSet<ShopScheduleInterval> ShopScheduleIntervals { get; set; }
    
    public virtual DbSet<FavoriteShop> FavoriteShops { get; set; }
    public virtual DbSet<Shop> Shops { get; set; }
    
    public virtual DbSet<ShopContact> ShopContacts { get; set; }
    public virtual DbSet<ShopPhoto> ShopPhotos { get; set; }
    
    public virtual DbSet<City> Cities { get; set; }
    
    public virtual DbSet<Location> Locations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShopContact>(entity =>
        {
            entity.HasOne(sc => sc.Shop)
                .WithOne(s => s.ShopContact)
                .HasForeignKey<ShopContact>(sc => sc.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasIndex(s => s.ShopContactId);
        });
        
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasOne(sc => sc.Shop)
                .WithOne(s => s.Location)
                .HasForeignKey<Location>(sc => sc.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasIndex(s => s.LocationId);
        });
    }
}