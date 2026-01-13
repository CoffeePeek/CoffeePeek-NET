using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Moderation.Infrastructure;

public class ModerationDbContext(DbContextOptions<ModerationDbContext> options) : DbContext(options)
{
    public DbSet<ModerationShop> ModerationShops { get; set; }
    public DbSet<ModerationReview> ModerationReviews { get; set; }
    public DbSet<PhotoMetadata> ShopPhotos { get; set; }
    public DbSet<ModerationShopEquipment> ModerationShopEquipments { get; set; }
    public DbSet<ModerationCoffeeBeanShop> ModerationCoffeeBeanShops { get; set; }
    public DbSet<ModerationShopRoaster> ModerationRoasterShops { get; set; }
    public DbSet<ModerationShopBrewMethod> ModerationShopBrewMethods { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ModerationReview>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(mr => mr.Id);
            
            entity.HasIndex(mr => mr.ShopId);
            entity.HasIndex(mr => mr.UserId);
            entity.HasIndex(mr => mr.ModeratedBy);
            entity.HasIndex(mr => mr.ModerationStatus);

            entity.Property(mr => mr.Header).HasMaxLength(BusinessConstants.MaxReviewHeaderLength);
            entity.Property(mr => mr.Comment).HasMaxLength(BusinessConstants.MaxReviewCommentLength);
            entity.Property(mr => mr.RejectedReason).HasMaxLength(BusinessConstants.MaxRejectReasonCommentLength);
        });
        
        modelBuilder.Entity<ModerationShop>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.RejectedReason).HasMaxLength(200);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ModerationStatus);
            entity.HasIndex(e => e.CityId);

            entity.OwnsOne(e => e.Contact, contact =>
            {
                contact.Property(sc => sc.PhoneNumber).HasMaxLength(BusinessConstants.MaxShopContactPhoneNumberLength);
                contact.Property(sc => sc.Email).HasMaxLength(BusinessConstants.MaxShopContactEmailLength);
                contact.Property(sc => sc.SiteLink).HasMaxLength(BusinessConstants.MaxShopContactSiteLinkLength);
                contact.Property(sc => sc.InstagramLink)
                    .HasMaxLength(BusinessConstants.MaxShopContactInstagramLinkLength);
            });

            entity.OwnsOne(e => e.Location, location =>
            {
                location.HasIndex(l => new { l.Latitude, l.Longitude });
            });

            entity.OwnsMany(e => e.Schedules, schedules =>
            {
                schedules.OwnsMany(sc => sc.Intervals, intervals =>
                {
                    intervals.Property(i => i.OpenTime).IsRequired();
                    intervals.Property(i => i.CloseTime).IsRequired();
                });
            });
            
            entity.HasMany(e => e.ShopPhotos)
                .WithOne()
                .HasForeignKey(x => x.ModerationShopId)
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
        
        modelBuilder.Entity<PhotoMetadata>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(e => e.Id);
        });
        
        modelBuilder.Entity<ModerationShopEquipment>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.EquipmentId);
        });
        
        modelBuilder.Entity<ModerationCoffeeBeanShop>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.CoffeeBeanId);
        });
        
        modelBuilder.Entity<ModerationShopRoaster>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.RoasterId);
        });
        
        modelBuilder.Entity<ModerationShopBrewMethod>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.BrewMethodId);
        });
    }
}

