using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.MediaService.Data;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<PhotoMetadata> Photos => Set<PhotoMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MassTransitOutbox(modelBuilder);
        
        modelBuilder.Entity<PhotoMetadata>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();

            entity.Property(p => p.StorageKey).IsRequired().HasMaxLength(255);
            entity.HasIndex(p => p.StorageKey).IsUnique();

            entity.Property(p => p.FileName).IsRequired().HasMaxLength(255);
            entity.Property(p => p.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(p => p.SizeBytes).IsRequired();
            entity.Property(p => p.BucketType).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.OwnerType).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.OwnerId).IsRequired();
            entity.Property(p => p.UploadedAt).IsRequired();
            entity.Property(p => p.PermalinkExpiresAt);
            entity.Property(p => p.ScheduledDeletionAt);
            entity.Property(p => p.DeletedAt);

            entity.HasIndex(p => new { p.OwnerType, p.OwnerId });
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => new { p.Status, p.ScheduledDeletionAt });
        });

        base.OnModelCreating(modelBuilder);
    }
    
    private static void MassTransitOutbox(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
