using CoffeePeek.Domain.Databases.Extensions;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Review;
using CoffeePeek.Domain.Entities.Schedules;
using CoffeePeek.Domain.Entities.Shop;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Domain.Databases;

public class CoffeePeekDbContext(DbContextOptions<CoffeePeekDbContext> options)
    : DbContext
{
    public virtual DbSet<Shop> Shops { get; set; }
    public virtual DbSet<ShopContacts> ShopContacts { get; set; }
    
    public virtual DbSet<Review> Reviews { get; set; }
    
    public virtual DbSet<Schedule> Schedules { get; set; }
    public virtual DbSet<ScheduleException> ScheduleExceptions { get; set; }
    
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<Street> Streets { get; set; }
    
    public virtual DbSet<ModerationShop> ModerationShops { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddressConfigure();
        modelBuilder.ReviewConfigure();
        modelBuilder.ScheduleConfigure();
        modelBuilder.ShopConfigure();
        
        base.OnModelCreating(modelBuilder);
    }
}