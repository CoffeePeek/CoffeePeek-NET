using CoffeePeek.Domain.Databases.Extensions;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Review;
using CoffeePeek.Domain.Entities.Schedules;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Domain.Databases;

public class CoffeePeekDbContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    
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
    
    public CoffeePeekDbContext(DbContextOptions<CoffeePeekDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>();
        modelBuilder.Entity<Role>();
        modelBuilder.Entity<UserRole>();

        modelBuilder.AddressConfigure();
        modelBuilder.ReviewConfigure();
        modelBuilder.ScheduleConfigure();
        modelBuilder.ShopConfigure();
        
        base.OnModelCreating(modelBuilder);
    }
}