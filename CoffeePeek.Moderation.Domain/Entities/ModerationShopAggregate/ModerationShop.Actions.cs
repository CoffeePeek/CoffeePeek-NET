using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed partial class ModerationShop
{
    public static ModerationShop Create(
        string name,
        Guid userId,
        Guid cityId,
        PriceRange priceRange,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");

        return new ModerationShop
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Name = name,
            UserId = userId,
            CityId = cityId,
            PriceRange = priceRange,
            Description = description,
            ModerationStatus = ModerationStatus.Pending,
        };
    }

    public void SetLocation(ModerationLocation moderationLocation)
    {
        Location = moderationLocation;
        LocationId = moderationLocation.Id;
    }

    public void AddPhoto(string fileName, string contentType, string storageKey, long length)
    {
        var photo = new PhotoMetadata(fileName, contentType, storageKey, length, UserId, Id);
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
        ModerationShopContact ??= ModerationShopContact.Create(Id, phone, instagramLink: instagram, email, site);
        ModerationShopContact.Update(phone, email, instagram, site);
    }

    public void UpdateSchedules(IEnumerable<ScheduleDto> dtos)
    {
        _schedules.Clear();
        foreach (var dto in dtos)
        {
            _schedules.Add(new ModerationShopSchedule(dto.DayOfWeek, dto.Intervals));
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
}