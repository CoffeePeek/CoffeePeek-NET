using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Persistance.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Tests.Persistence;

public class RoasterPhotoPersistenceTests
{
    [Fact]
    public void Model_UsesMediaServiceCompatibleMetadataLimits()
    {
        var options = new DbContextOptionsBuilder<ShopsDbContext>()
            .UseNpgsql("Host=localhost;Database=shops;Username=postgres;Password=postgres")
            .Options;

        using var dbContext = new ShopsDbContext(options);
        var entityType = dbContext.Model.FindEntityType(typeof(RoasterPhoto));

        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(RoasterPhoto.FileName))!.GetMaxLength()
            .Should().Be(BusinessConstants.MaxRoasterPhotoFileNameLength);
        entityType.FindProperty(nameof(RoasterPhoto.ContentType))!.GetMaxLength()
            .Should().Be(BusinessConstants.MaxRoasterPhotoContentTypeLength);
        entityType.FindProperty(nameof(RoasterPhoto.StorageKey))!.GetMaxLength()
            .Should().Be(BusinessConstants.MaxRoasterPhotoStorageKeyLength);
    }
}
