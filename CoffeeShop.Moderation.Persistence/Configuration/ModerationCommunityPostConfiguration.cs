using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShop.Moderation.Persistence.Configuration;

public class ModerationCommunityPostConfiguration : IEntityTypeConfiguration<ModerationCommunityPost>
{
    public void Configure(EntityTypeBuilder<ModerationCommunityPost> entity)
    {
        entity.UsePropertyAccessMode(PropertyAccessMode.Field);
        entity.HasKey(p => p.Id);
        entity.HasIndex(p => p.UserId);
        entity.HasIndex(p => p.ModerationStatus);
        entity.HasIndex(p => p.LinkedShopId);

        entity.Property(p => p.UserName).HasMaxLength(30);
        entity.Property(p => p.Title).HasMaxLength(BusinessConstants.MaxCommunityPostTitleLength);
        entity.Property(p => p.Body).HasMaxLength(BusinessConstants.MaxCommunityPostBodyLength);
        entity.Property(p => p.RejectedReason).HasMaxLength(BusinessConstants.MaxRejectReasonCommentLength);
    }
}
