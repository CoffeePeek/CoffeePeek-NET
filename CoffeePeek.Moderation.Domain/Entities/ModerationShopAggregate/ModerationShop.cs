using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public partial class ModerationShop : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PriceRange? PriceRange { get; private set; }
    public ModerationStatus ModerationStatus { get; private set; }
    public string? RejectedReason { get; private set; }
    
    public Guid UserId { get; private init; }
    public Guid CityId { get; private set; }
    public Guid ShopId { get; private set; }
    
    public ModerationShopContact Contact { get; private set; }
    public ModerationLocation Location { get; private set; }

    private readonly List<PhotoMetadata> _shopPhotos = [];
    public IReadOnlyCollection<PhotoMetadata> ShopPhotos => _shopPhotos.AsReadOnly();

    private readonly List<ModerationShopSchedule> _schedules = [];
    public IReadOnlyCollection<ModerationShopSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<ModerationShopEquipment> _moderationShopEquipments = [];
    public IReadOnlyCollection<ModerationShopEquipment> ModerationShopEquipments => _moderationShopEquipments.AsReadOnly();

    private readonly List<ModerationCoffeeBeanShop> _moderationCoffeeBeanShops = [];
    public IReadOnlyCollection<ModerationCoffeeBeanShop> ModerationCoffeeBeanShops => _moderationCoffeeBeanShops.AsReadOnly();
    private readonly List<ModerationShopRoaster> _moderationRoasterShops = [];
    public IReadOnlyCollection<ModerationShopRoaster> ModerationRoasterShops => _moderationRoasterShops.AsReadOnly();
    private readonly List<ModerationShopBrewMethod> _moderationShopBrewMethods = [];
    public IReadOnlyCollection<ModerationShopBrewMethod> ModerationShopBrewMethods => _moderationShopBrewMethods.AsReadOnly();

    private ModerationShop() { }
}
