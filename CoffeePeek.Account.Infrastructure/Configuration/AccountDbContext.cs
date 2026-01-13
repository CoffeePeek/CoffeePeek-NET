using CoffeePeek.Account.Domain;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using OutboxEvent = CoffeePeek.Account.Domain.Entities.OutboxEvent;

namespace CoffeePeek.Auth.Infrastructure.Configuration;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    
    public DbSet<User> Users { get; set; }
    public DbSet<PhotoMetadata> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.Entity<PhotoMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StorageKey).IsRequired();
        });
        
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity
                .HasOne(x => x.User)
                .WithMany(uc => uc.RefreshTokens)
                .HasForeignKey(x => x.UserId);
            
            entity.Property(x => x.Token).HasMaxLength(BusinessConstants.MaxRefreshTokenLength).IsRequired();
            entity.Property(x => x.DeviceName).HasMaxLength(BusinessConstants.MaxDeviceNameLength);
            entity.Property(x => x.IpAddress).HasMaxLength(BusinessConstants.MaxIpAddressLength);
        });
    }
}