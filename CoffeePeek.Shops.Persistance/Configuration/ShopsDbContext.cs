using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityFollowAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;

namespace CoffeePeek.Shops.Persistance.Configuration;

public class ShopsDbContext(DbContextOptions<ShopsDbContext> options) : DbContext(options)
{
    //category
    public virtual DbSet<BrewMethod> BrewMethods { get; set; }
    
    public virtual DbSet<CoffeeBean> CoffeeBeans { get; set; }
    
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<CheckIn> CheckIns { get; set; }
    public virtual DbSet<CommunityComment> CommunityComments { get; set; }
    public virtual DbSet<CommunityPost> CommunityPosts { get; set; }
    public virtual DbSet<CommunityReaction> CommunityReactions { get; set; }
    public virtual DbSet<CommunityUserFollow> CommunityUserFollows { get; set; }
    public virtual DbSet<CommunityCityFollow> CommunityCityFollows { get; set; }
    
    public virtual DbSet<Roaster> Roasters { get; set; }
    
    public virtual DbSet<UserFavorite> UserFavorites { get; set; }
    public virtual DbSet<CoffeeShop> Shops { get; set; }
    
    public virtual DbSet<ShopPhoto> ShopPhotos { get; set; }
    
    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<EquipmentCategory> EquipmentCategories { get; set; }
    public virtual DbSet<Equipment> Equipments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfiguration(new CoffeeShopConfiguration());
        
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.CoffeeShopId);
            entity.HasIndex(r => r.UserId);
            entity.Property(r => r.CoffeeShopId).IsRequired();

            entity.Property(r => r.Header).HasMaxLength(BusinessConstants.MaxReviewHeaderLength);
            entity.Property(r => r.Comment).HasMaxLength(BusinessConstants.MaxReviewCommentLength);
            entity.Property(r => r.UserName).IsRequired().HasMaxLength(30);

            // Foreign key without navigation property (DDD aggregate separation)
            entity.HasOne<CoffeeShop>()
                .WithMany()
                .HasForeignKey(r => r.CoffeeShopId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne<Rating>(r => r.Rating);
        });
        
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.ShopId);
            entity.HasIndex(c => c.ReviewId);
            entity.HasIndex(c => new { c.UserId, c.ShopId });
            
            entity.Property(c => c.ShopId).IsRequired();
            entity.Property(c => c.ReviewId).IsRequired(false);
                
            entity.Property(c => c.Note).HasMaxLength(BusinessConstants.MaxCheckInNoteLength);
            
            // Foreign keys without navigation properties (DDD aggregate separation)
            entity.HasOne<CoffeeShop>()
                .WithMany()
                .HasForeignKey(c => c.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Review>()
                .WithMany()
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.OwnsOne<Rating>(r => r.Rating);
        });

        modelBuilder.Entity<CommunityComment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => new { c.TargetType, c.TargetId });
            entity.HasIndex(c => c.ParentCommentId);
            entity.HasIndex(c => c.UserId);

            entity.Property(c => c.UserName).IsRequired().HasMaxLength(30);
            entity.Property(c => c.Body).IsRequired().HasMaxLength(BusinessConstants.MaxCommunityCommentBodyLength);
            entity.Property(c => c.TargetType).IsRequired();
            entity.Property(c => c.TargetId).IsRequired();

            entity.HasOne<CommunityComment>()
                .WithMany()
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommunityPost>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.LinkedShopId);
            entity.HasIndex(p => p.ModerationPostId).IsUnique();

            entity.Property(p => p.UserName).IsRequired().HasMaxLength(30);
            entity.Property(p => p.Title).IsRequired().HasMaxLength(BusinessConstants.MaxCommunityPostTitleLength);
            entity.Property(p => p.Body).IsRequired().HasMaxLength(BusinessConstants.MaxCommunityPostBodyLength);
            entity.Property(p => p.ModerationPostId).IsRequired();
        });

        modelBuilder.Entity<CommunityReaction>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.UserId, r.TargetType, r.TargetId }).IsUnique();
            entity.HasIndex(r => new { r.TargetType, r.TargetId });

            entity.Property(r => r.UserId).IsRequired();
            entity.Property(r => r.TargetType).IsRequired();
            entity.Property(r => r.TargetId).IsRequired();
            entity.Property(r => r.ReactionType).IsRequired();
        });

        modelBuilder.Entity<CommunityUserFollow>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new { f.FollowerId, f.FollowingUserId }).IsUnique();
            entity.HasIndex(f => f.FollowerId);
            entity.HasIndex(f => f.FollowingUserId);

            entity.Property(f => f.FollowerId).IsRequired();
            entity.Property(f => f.FollowingUserId).IsRequired();
        });

        modelBuilder.Entity<CommunityCityFollow>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new { f.UserId, f.CityId }).IsUnique();
            entity.HasIndex(f => f.UserId);
            entity.HasIndex(f => f.CityId);

            entity.Property(f => f.UserId).IsRequired();
            entity.Property(f => f.CityId).IsRequired();
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.CoffeeShopId }).IsUnique();

            entity.HasIndex(f => f.UserId);

            entity.HasIndex(f => f.CoffeeShopId);

            entity.Property(f => f.UserId).IsRequired();
            entity.Property(f => f.CoffeeShopId).IsRequired();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CategoryId);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<EquipmentCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(BusinessConstants.MaxEquipmentCategoryNameLength);
        });
    }
}