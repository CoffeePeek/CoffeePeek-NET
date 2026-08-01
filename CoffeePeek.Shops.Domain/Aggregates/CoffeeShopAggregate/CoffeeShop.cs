using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain.Aggregates.BrewMethods;
using CoffeePeek.Shops.Domain.Entities;
using FluentResults;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public sealed class CoffeeShop : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PriceRange PriceRange { get; private set; }
    public CoffeeShopStatus Status { get; private set; } = CoffeeShopStatus.Active;

    public Guid CreatorId { get; private set; }
    public Guid? OwnerUserId { get; private set; }
    public Guid? ModerationId {get; private set; }

    public ShopContact Contact { get; private set; }
    public Location Location { get; private set; }

    private readonly List<ShopSchedule> _schedules = [];
    public IReadOnlyCollection<ShopSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<ShopPhoto> _shopPhotos = [];
    public IReadOnlyCollection<ShopPhoto> ShopPhotos => _shopPhotos.AsReadOnly();

    private readonly List<Equipment> _equipments = [];
    public IReadOnlyCollection<Equipment> Equipments => _equipments.AsReadOnly();

    private readonly List<CoffeeBean> _coffeeBeans = [];
    public IReadOnlyCollection<CoffeeBean> CoffeeBeans => _coffeeBeans.AsReadOnly();

    private readonly List<Roaster> _roasters = [];
    public IReadOnlyCollection<Roaster> Roasters => _roasters.AsReadOnly();

    private readonly List<BrewMethod> _brewMethods = [];
    public IReadOnlyCollection<BrewMethod> BrewMethods => _brewMethods.AsReadOnly();

    private readonly List<CoffeeShopTag> _shopTags = [];
    public IReadOnlyCollection<CoffeeShopTag> ShopTags => _shopTags.AsReadOnly();

    // ReSharper disable once UnusedMember.Local
    private CoffeeShop()
    {
    }

    public CoffeeShop(Guid creatorId, string name, string? description, PriceRange priceRange, Guid moderationId)
    {
        Id = Guid.NewGuid();
        CreatorId = creatorId;
        Name = name;
        Description = description;
        PriceRange = priceRange;
        ModerationId = moderationId;
    }

    #region Domain Logic

    public bool IsNew => CreatedAtUtc > DateTime.UtcNow.AddDays(-BusinessConstants.ItNewEntityInDays);
    public bool IsOpen => IsOpenAt(DateTime.UtcNow);
    
    private bool IsOpenAt(DateTime dateTime)
    {
        switch (Status)
        {
            case CoffeeShopStatus.PermanentlyClosed:
            case CoffeeShopStatus.TemporarilyClosed:
                return false;
            case CoffeeShopStatus.Active:
                break;
        }

        if (Schedules.Count == 0)
            return true;
            
        var daySchedule = Schedules.FirstOrDefault(s => s.DayOfWeek == dateTime.DayOfWeek);
        
        if (daySchedule == null)
            return false;
            
        if (daySchedule.IsClosed)
            return false;
            
        var currentTime = dateTime.TimeOfDay;
        
        return daySchedule.Intervals.Any(interval => 
            currentTime >= interval.OpenTime && 
            currentTime <= interval.CloseTime);
    }
    
    public void UpdateDetails(string name, string? description, PriceRange priceRange)
    {
        Name = name;
        Description = description;
        PriceRange = priceRange;
    }

    public void SetStatus(CoffeeShopStatus status)
    {
        Status = status;
    }

    public void SetHidden(bool hidden)
    {
        Status = hidden ? CoffeeShopStatus.TemporarilyClosed : CoffeeShopStatus.Active;
    }

    public void AssignOwner(Guid? ownerUserId)
    {
        OwnerUserId = ownerUserId == Guid.Empty ? null : ownerUserId;
    }

    public void SetLocation(Guid cityId, string address, decimal latitude, decimal longitude)
    {
        Location = Location.CreateValidated(cityId, address, latitude, longitude);
    }

    public void SetContact(string? instagramLink, string? email, string? siteLink, string? phoneNumber)
    {
        Contact = ShopContact.Create(instagramLink, email, siteLink, phoneNumber);
    }
    
    public void AddPhotos(IEnumerable<ShopPhoto> photos)
    {
        var nextIndex = _shopPhotos.Count == 0
            ? 0
            : _shopPhotos.Max(p => p.SortIndex) + 1;

        foreach (var photo in photos)
        {
            photo.SetSortIndex(nextIndex++);
            _shopPhotos.Add(photo);
        }
    }

    /// <summary>
    /// Reorders gallery photos. <paramref name="orderedPhotoIds"/> must be a full permutation
    /// of the shop's current photo IDs; first ID becomes SortIndex 0 (cover).
    /// </summary>
    public Result ReorderPhotos(IReadOnlyList<Guid> orderedPhotoIds)
    {
        if (orderedPhotoIds is null)
            return Result.Fail("Photo order is required.");

        if (orderedPhotoIds.Count != _shopPhotos.Count)
            return Result.Fail("Photo order must include every gallery photo exactly once.");

        if (orderedPhotoIds.Distinct().Count() != orderedPhotoIds.Count)
            return Result.Fail("Photo order contains duplicate photo IDs.");

        var byId = _shopPhotos.ToDictionary(p => p.Id);
        foreach (var id in orderedPhotoIds)
        {
            if (!byId.ContainsKey(id))
                return Result.Fail($"Photo '{id}' does not belong to this shop.");
        }

        for (var i = 0; i < orderedPhotoIds.Count; i++)
            byId[orderedPhotoIds[i]].SetSortIndex(i);

        return Result.Ok();
    }
    
    public void AddEquipment(Equipment equipment)
    {
        if (_equipments.Any(e => e.Brand == equipment.Brand && e.ModelName == equipment.ModelName))
            return;
        
        if (equipment.IsPrimary)
        {
            foreach (var e in _equipments.Where(e => e.CategoryId == equipment.CategoryId))
            {
                e.UnmarkAsPrimary();
            }
        }

        _equipments.Add(equipment);
    }

    public void RemoveEquipment(Guid equipmentId)
    {
        var equipment = _equipments.FirstOrDefault(e => e.Id == equipmentId);
        if (equipment != null)
        {
            _equipments.Remove(equipment);
        }
    }
    
    public void SetBrewMethods(IEnumerable<BrewMethod> methods)
    {
        _brewMethods.Clear();
        _brewMethods.AddRange(methods);
    }

    public void SetRoasters(IEnumerable<Roaster> roasters)
    {
        _roasters.Clear();
        _roasters.AddRange(roasters);
    }

    public void SetBeans(IEnumerable<CoffeeBean> beans)
    {
        _coffeeBeans.Clear();
        _coffeeBeans.AddRange(beans);
    }
    
    public void AddSchedule(List<ShopSchedule> schedule)
    {
        _schedules.AddRange(schedule);
    }

    /// <summary>
    /// Replaces the shop's tag set. Tag IDs are deduplicated; max
    /// <see cref="BusinessConstants.MaxShopTagsPerShop"/> enforced.
    /// </summary>
    public void SetTags(IReadOnlyList<Guid> tagIds, Guid assignedByUserId)
    {
        ArgumentNullException.ThrowIfNull(tagIds);

        if (assignedByUserId == Guid.Empty)
            throw new DomainException("AssignedByUserId is required.");

        var distinct = tagIds.Distinct().ToList();
        if (distinct.Count > BusinessConstants.MaxShopTagsPerShop)
            throw new DomainException(
                $"A shop cannot have more than {BusinessConstants.MaxShopTagsPerShop} tags.");

        _shopTags.Clear();
        var now = DateTime.UtcNow;
        foreach (var tagId in distinct)
        {
            if (tagId == Guid.Empty)
                throw new DomainException("TagId cannot be empty.");

            _shopTags.Add(new CoffeeShopTag(Id, tagId, assignedByUserId, now));
        }
    }
    
    #endregion
}