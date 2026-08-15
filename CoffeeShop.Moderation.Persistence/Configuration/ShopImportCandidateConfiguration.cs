using System.Text.Json;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeeShop.Moderation.Persistence.Configuration;

public class ShopImportCandidateConfiguration : IEntityTypeConfiguration<ShopImportCandidate>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<ShopImportCandidate> entity)
    {
        entity.ToTable("ShopImportCandidates");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.ExternalId).HasMaxLength(64).IsRequired();
        entity.Property(e => e.Name).HasMaxLength(200);
        entity.Property(e => e.Address).HasMaxLength(500);
        entity.Property(e => e.Phone).HasMaxLength(40);
        entity.Property(e => e.Website).HasMaxLength(2048);
        entity.Property(e => e.Instagram).HasMaxLength(255);
        entity.Property(e => e.OpeningHours).HasMaxLength(500);
        entity.Property(e => e.Cuisine).HasMaxLength(200);
        entity.Property(e => e.Brand).HasMaxLength(200);
        entity.Property(e => e.CheckDate).HasMaxLength(32);
        entity.Property(e => e.GoogleMapsUri).HasMaxLength(2048);
        entity.Property(e => e.Latitude).HasPrecision(18, 10);
        entity.Property(e => e.Longitude).HasPrecision(18, 10);

        entity.Property(e => e.Signals)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(StringListComparer());

        entity.Property(e => e.TagSlugs)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(StringListComparer());

        entity.HasIndex(e => new { e.Source, e.ExternalId }).IsUnique();
        entity.HasIndex(e => e.QueueStatus);
        entity.HasIndex(e => e.CollectorBucket);
        entity.HasIndex(e => e.CoffeeFocus);
    }

    private static ValueComparer<List<string>> StringListComparer() =>
        new(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            v => v.ToList());
}
