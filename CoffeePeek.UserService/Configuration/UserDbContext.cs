using CoffeePeek.UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Configuration;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserStatistics> UserStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserStatistics>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.CheckInCount).HasDefaultValue(0);
            entity.Property(e => e.ReviewCount).HasDefaultValue(0);
            entity.Property(e => e.AddedShopsCount).HasDefaultValue(0);
        });
    }
}