using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShop.Moderation.Persistence.Configuration;

public class ModerationAuditLogConfiguration : IEntityTypeConfiguration<ModerationAuditLog>
{
    public void Configure(EntityTypeBuilder<ModerationAuditLog> builder)
    {
        builder.ToTable("ModerationAuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(BusinessConstants.MaxRejectReasonCommentLength);
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
