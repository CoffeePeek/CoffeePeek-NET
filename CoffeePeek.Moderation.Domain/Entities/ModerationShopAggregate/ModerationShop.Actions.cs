using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed partial class ModerationShop
{
    public static ModerationShop Create(
        string name,
        Guid userId,
        Guid cityId,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");

        return new ModerationShop
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            CityId = cityId,
            Description = description,
            ModerationStatus = ModerationStatus.Pending,
        };
    }

    public void SetLocation(ModerationLocation moderationLocation)
    {
        Location = moderationLocation;
    }

    public void AddPhoto(string fileName, string contentType, string storageKey, long length)
    {
        var photo = PhotoMetadata.Create(fileName, contentType, storageKey, length, UserId, Id);
        _shopPhotos.Add(photo);
    }

    public void UpdateInfo(string? name, string? description, PriceRange? priceRange, Guid? cityId)
    {
        if (name != null) Name = name;
        if (description != null) Description = description;
        if (priceRange.HasValue) PriceRange = priceRange.Value;
        if (cityId.HasValue) CityId = cityId.Value;
    }

    public void UpdateContacts(string? phone, string? instagram, string? email, string? site)
    {
        Contact = ModerationShopContact.Create(phone, instagramLink: instagram, email, site);
    }

    public void UpdateSchedules(IEnumerable<(DayOfWeek DayOfWeek, List<(TimeSpan OpenTime, TimeSpan CloseTime)> Intervals)> schedules)
    {
        _schedules.Clear();
    
        foreach (var schedule in schedules)
        {
            var intervals = schedule.Intervals
                .Select(i => new ModerationShopScheduleInterval(i.OpenTime, i.CloseTime))
                .ToList()
                .AsReadOnly();
        
            var isClosed = schedule.Intervals.Count == 0;
        
            _schedules.Add(new ModerationShopSchedule(schedule.DayOfWeek, isClosed, intervals));
        }
    }

    public void UpdateRelations(
        List<Guid>? equipmentIds,
        List<Guid>? coffeeBeanIds,
        List<Guid>? roasterIds,
        List<Guid>? brewMethodIds)
    {
        UpdateCollection(_moderationShopEquipments, equipmentIds,
            id => new ModerationShopEquipment(Id, id),
            e => e.EquipmentId);
        
        UpdateCollection(_moderationCoffeeBeanShops, coffeeBeanIds, 
            id => new ModerationCoffeeBeanShop(Id, id),
            e => e.CoffeeBeanId);
        
        UpdateCollection(_moderationRoasterShops, roasterIds, 
            id => new ModerationShopRoaster(Id, id),
            e => e.RoasterId);
        
        UpdateCollection(_moderationShopBrewMethods, brewMethodIds, 
            id => new ModerationShopBrewMethod(Id, id),
            e => e.BrewMethodId);
    }
    
    public void Approve()
    {
        if (ModerationStatus == ModerationStatus.Approved) return;

        if (!Location!.IsAddressValidated)
            throw new DomainException("Cannot approve shop with unvalidated address.");

        ModerationStatus = ModerationStatus.Approved;
    }

    public void Reject(string reason)
    {
        ModerationStatus = ModerationStatus.Rejected;
        RejectedReason = reason;
    }

    public void SetShopId(Guid shopId)
    {
        if (shopId == Guid.Empty)
            throw new DomainException($"{nameof(shopId)} cannot be empty.");

        if (ShopId == shopId)
            return;

        ShopId = shopId;
    }
    
    private static void UpdateCollection<TJoinEntity>(
        List<TJoinEntity> currentCollection,
        IEnumerable<Guid>? newIds,
        Func<Guid, TJoinEntity> createFunc,
        Func<TJoinEntity, Guid> getIdFunc)
    {
        if (newIds == null) return;

        var newIdSet = newIds.ToHashSet();

        currentCollection.RemoveAll(item => !newIdSet.Contains(getIdFunc(item)));

        var currentIdSet = currentCollection.Select(getIdFunc).ToHashSet();
        var idsToAdd = newIdSet.Where(id => !currentIdSet.Contains(id));

        currentCollection.AddRange(idsToAdd.Select(createFunc));
    }

    public void AddShopId(Guid shopId)
    {
        if (shopId == Guid.Empty) 
            return;
        
        ShopId = shopId;
    }

    public void AddPriceRange(PriceRange priceRange)
    {
        PriceRange = priceRange;
    }
}