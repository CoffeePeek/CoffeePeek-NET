using CoffeePeek.Account.Domain.Aggregates;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.UserService.Models;
using Microsoft.EntityFrameworkCore;
using OutboxEvent = CoffeePeek.Account.Domain.Entities.OutboxEvent;

namespace CoffeePeek.Auth.Infrastructure.Persistent;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options)
{
    public DbSet<UserCredential> UserCredentials { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    
    public DbSet<User> Users { get; set; }
    public DbSet<UserStatistics> UserStatistics { get; set; }
    public DbSet<PhotoMetadata> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.UserCredential)
                .WithOne(uc => uc.User)
                .HasForeignKey<User>(u => u.UserCredentialId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(u => u.PhotoMetadata)
                .WithMany() 
                .HasForeignKey(u => u.PhotoMetadataId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PhotoMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StorageKey).IsRequired();
        });
        
        modelBuilder.Entity<RefreshToken>()
            .HasOne(x => x.UserCredential)
            .WithMany(uc => uc.RefreshTokens)
            .HasForeignKey(x => x.UserCredentialId);
        
        modelBuilder.Entity<UserStatistics>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.CheckInCount).HasDefaultValue(0);
            entity.Property(e => e.ReviewCount).HasDefaultValue(0);
            entity.Property(e => e.AddedShopsCount).HasDefaultValue(0);
            
            entity.HasOne(us => us.User)
                .WithOne(u => u.UserStatistics)
                .HasForeignKey<UserStatistics>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });
    }
}