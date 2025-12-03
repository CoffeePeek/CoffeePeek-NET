using CoffeePeek.ModerationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ModerationService.Configuration;

public class ModerationDbContext(DbContextOptions<ModerationDbContext> options) : DbContext(options)
{
    public DbSet<ModerationShop> ModerationShops { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<ShopContacts> ShopContacts { get; set; }
    public DbSet<ShopPhoto> ShopPhotos { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<ScheduleException> ScheduleExceptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ModerationShop>(entity =>
        {
            entity.ToTable("ModerationShops");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NotValidatedAddress).HasMaxLength(150);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ModerationStatus);
            
            entity.HasOne(e => e.Address)
                .WithMany()
                .HasForeignKey(e => e.AddressId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.ShopContacts)
                .WithMany()
                .HasForeignKey(e => e.ShopContactId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasMany(e => e.ShopPhotos)
                .WithOne()
                .HasForeignKey("ShopId")
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Schedules)
                .WithOne()
                .HasForeignKey("ShopId")
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.ScheduleExceptions)
                .WithOne()
                .HasForeignKey("ShopId")
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.HasKey(e => e.Id);
        });
        
        modelBuilder.Entity<ShopContacts>(entity =>
        {
            entity.ToTable("ShopContacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhoneNumber).HasMaxLength(18);
            entity.Property(e => e.InstagramLink).HasMaxLength(50);
        });
        
        modelBuilder.Entity<ShopPhoto>(entity =>
        {
            entity.ToTable("ShopPhotos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(70);
        });
        
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedules");
            entity.HasKey(e => e.Id);
        });
        
        modelBuilder.Entity<ScheduleException>(entity =>
        {
            entity.ToTable("ScheduleExceptions");
            entity.HasKey(e => e.Id);
        });
    }
}

