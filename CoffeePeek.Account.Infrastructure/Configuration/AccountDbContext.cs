using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.UserService.Models;
using Microsoft.EntityFrameworkCore;
using OutboxEvent = CoffeePeek.Account.Domain.Entities.OutboxEvent;

namespace CoffeePeek.Auth.Infrastructure.Configuration;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options)
{
    public DbSet<UserCredential> UserCredentials { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    
    public DbSet<Account.Domain.Entities.User> Users { get; set; }
    public DbSet<UserStatistics> UserStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User <-> UserCredential (one-to-one)
        modelBuilder.Entity<Account.Domain.Entities.User>()
            .HasOne(u => u.UserCredential)
            .WithOne(uc => uc.User)
            .HasForeignKey<Account.Domain.Entities.User>(u => u.UserCredentialId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // User <-> UserStatistics (one-to-one)
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
        
        // UserRole (many-to-many)
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
    }
}