using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Moderation.Infrastructure.Configuration;

public class ModerationReviewConfiguration : IEntityTypeConfiguration<ModerationReview>
{
    public void Configure(EntityTypeBuilder<ModerationReview> entity)
    {
        entity.UsePropertyAccessMode(PropertyAccessMode.Field);
        entity.HasKey(mr => mr.Id);
        entity.HasOne(mr => mr.ModerationShop);
            
        entity.HasIndex(mr => mr.ShopId);
        entity.HasIndex(mr => mr.UserId);
        entity.HasIndex(mr => mr.ModeratedBy);
        entity.HasIndex(mr => mr.ModerationStatus);

        entity.Property(mr => mr.UserName).HasMaxLength(30);
        entity.Property(mr => mr.Header).HasMaxLength(BusinessConstants.MaxReviewHeaderLength);
        entity.Property(mr => mr.Comment).HasMaxLength(BusinessConstants.MaxReviewCommentLength);
        entity.Property(mr => mr.RejectedReason).HasMaxLength(BusinessConstants.MaxRejectReasonCommentLength);
            
        entity.OwnsOne<Rating>(r => r.Rating);
    }
}