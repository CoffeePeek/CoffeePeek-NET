using CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Account.Persistence.Configuration;

public class CommunityNotificationConfiguration : IEntityTypeConfiguration<CommunityNotification>
{
    public void Configure(EntityTypeBuilder<CommunityNotification> entity)
    {
        entity.HasKey(n => n.Id);
        entity.HasIndex(n => n.UserId);
        entity.HasIndex(n => new { n.UserId, n.IsRead });

        entity.Property(n => n.Title).IsRequired().HasMaxLength(120);
        entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
        entity.Property(n => n.RelatedEntityType).HasMaxLength(50);
        entity.Property(n => n.DedupKey).HasMaxLength(200);
        entity.HasIndex(n => n.DedupKey).IsUnique();
        entity.Property(n => n.Type).IsRequired();
        entity.Property(n => n.UserId).IsRequired();
    }
}
