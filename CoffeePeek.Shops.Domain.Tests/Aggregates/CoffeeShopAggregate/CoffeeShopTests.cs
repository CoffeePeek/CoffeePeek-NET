using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain;
using FluentAssertions;
using JetBrains.Annotations;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.CoffeeShopAggregate;

[TestSubject(typeof(CoffeeShop))]
public class CoffeeShopTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var creatorId = Guid.NewGuid();
        var moderationId = Guid.NewGuid();

        var shop = new CoffeeShop(creatorId, "Test Shop", null, PriceRange.Moderate, moderationId);

        shop.Name.Should().Be("Test Shop");
        shop.CreatorId.Should().Be(creatorId);
        shop.Id.Should().NotBeEmpty();
        shop.Status.Should().Be(CoffeeShopStatus.Active);
    }

    [Fact]
    public void IsOpen_WhenActiveWithNoSchedule_ReturnsTrue()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());

        shop.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void AddPhotos_AssignsContiguousSortIndex()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();

        shop.AddPhotos(
        [
            new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId),
            new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId),
        ]);

        shop.ShopPhotos.Select(p => p.SortIndex).Should().Equal(0, 1);
    }

    [Fact]
    public void AddPhotos_CalledTwice_ContinuesSortIndex()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();

        shop.AddPhotos([new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId)]);
        shop.AddPhotos([new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId)]);

        shop.ShopPhotos.Select(p => p.SortIndex).Should().Equal(0, 1);
    }

    [Fact]
    public void ReorderPhotos_UpdatesSortIndexToMatchOrder()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();
        var first = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId);
        var second = new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId);
        var third = new ShopPhoto("c.jpg", "image/jpeg", "c", 1, ownerId);
        shop.AddPhotos([first, second, third]);

        var result = shop.ReorderPhotos([third.Id, first.Id, second.Id]);

        result.IsSuccess.Should().BeTrue();
        shop.ShopPhotos.Single(p => p.Id == third.Id).SortIndex.Should().Be(0);
        shop.ShopPhotos.Single(p => p.Id == first.Id).SortIndex.Should().Be(1);
        shop.ShopPhotos.Single(p => p.Id == second.Id).SortIndex.Should().Be(2);
    }

    [Fact]
    public void ReorderPhotos_WithMissingId_Fails()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var photo = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, Guid.NewGuid());
        shop.AddPhotos([photo]);

        var result = shop.ReorderPhotos([Guid.NewGuid()]);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void ReorderPhotos_WithIncompleteList_Fails()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();
        var first = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId);
        var second = new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId);
        shop.AddPhotos([first, second]);

        var result = shop.ReorderPhotos([first.Id]);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void ReorderPhotos_WithDuplicateIds_Fails()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();
        var first = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId);
        var second = new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId);
        shop.AddPhotos([first, second]);

        var result = shop.ReorderPhotos([first.Id, first.Id]);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void ReorderPhotos_EmptyGallery_Succeeds()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());

        var result = shop.ReorderPhotos([]);

        result.IsSuccess.Should().BeTrue();
        shop.ShopPhotos.Should().BeEmpty();
    }

    [Fact]
    public void ShopPhoto_Constructor_RejectsNegativeSortIndex()
    {
        var act = () => new ShopPhoto("a.jpg", "image/jpeg", "a", 1, Guid.NewGuid(), -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddEquipment_WithDuplicateBrandAndModel_DoesNotAddTwice()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var category = new EquipmentCategory();
        var e1 = new Equipment("Brand", "Model", category);
        var e2 = new Equipment("Brand", "Model", category);

        shop.AddEquipment(e1);
        shop.AddEquipment(e2);

        shop.Equipments.Count.Should().Be(1);
    }

    [Fact]
    public void SetTags_ReplacesDistinctTags()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var adminId = Guid.NewGuid();
        var tag1 = Guid.NewGuid();
        var tag2 = Guid.NewGuid();

        shop.SetTags([tag1, tag2, tag1], adminId);

        shop.ShopTags.Should().HaveCount(2);
        shop.ShopTags.Select(t => t.TagId).Should().BeEquivalentTo([tag1, tag2]);
        shop.ShopTags.Should().OnlyContain(t => t.AssignedByUserId == adminId && t.ShopId == shop.Id);
    }

    [Fact]
    public void SetTags_ExceedingMax_Throws()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var tooMany = Enumerable.Range(0, BusinessConstants.MaxShopTagsPerShop + 1)
            .Select(_ => Guid.NewGuid())
            .ToArray();

        var act = () => shop.SetTags(tooMany, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage($"*{BusinessConstants.MaxShopTagsPerShop}*");
    }

    [Fact]
    public void SetTags_EmptyList_ClearsTags()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        shop.SetTags([Guid.NewGuid()], Guid.NewGuid());

        shop.SetTags([], Guid.NewGuid());

        shop.ShopTags.Should().BeEmpty();
    }

    [Fact]
    public void SetCoffeeFocus_StoresValue()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Moderate, Guid.NewGuid());

        shop.SetCoffeeFocus(CoffeeFocus.Specialty);

        shop.CoffeeFocus.Should().Be(CoffeeFocus.Specialty);
    }

    [Fact]
    public void TryEnrichFromImport_FillsEmptyContactAndKeepsExistingPhone()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Coffe Joy", null, PriceRange.Moderate, Guid.NewGuid());
        shop.SetLocation(Guid.NewGuid(), "Минск", 53.9152m, 27.5847m);
        shop.SetContact(null, null, null, "+375291112233");

        var filled = shop.TryEnrichFromImport(
            "Немига 5, Минск",
            "https://instagram.com/coffejoy",
            "https://coffejoy.by",
            "+375 17 200-00-00");

        filled.Should().BeTrue();
        shop.Contact.PhoneNumber.Should().Be("+375291112233");
        shop.Contact.InstagramLink.Should().Be("https://instagram.com/coffejoy");
        shop.Contact.SiteLink.Should().Be("https://coffejoy.by");
        shop.Location.Address.Should().Be("Немига 5, Минск");
    }

    [Fact]
    public void TryEnrichFromImport_WhenNothingMissing_ReturnsFalse()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Coffe Joy", null, PriceRange.Moderate, Guid.NewGuid());
        shop.SetLocation(Guid.NewGuid(), "Немига 5, Минск", 53.9152m, 27.5847m);
        shop.SetContact("https://instagram.com/coffejoy", null, "https://coffejoy.by", "+375291112233");

        var filled = shop.TryEnrichFromImport("Немига 5, Минск", "@other", "https://other.by", "+375000000000");

        filled.Should().BeFalse();
        shop.Contact.InstagramLink.Should().Be("https://instagram.com/coffejoy");
        shop.Contact.PhoneNumber.Should().Be("+375291112233");
    }

    [Fact]
    public void MarkImportedFromFile_SetsTimestampOnce()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Surf Coffee", null, PriceRange.Moderate, Guid.NewGuid());
        var first = new DateTime(2026, 8, 26, 12, 0, 0, DateTimeKind.Utc);
        var second = first.AddHours(2);

        shop.MarkImportedFromFile(first);
        shop.MarkImportedFromFile(second);

        shop.ImportedFromFileAt.Should().Be(first);
    }
}
