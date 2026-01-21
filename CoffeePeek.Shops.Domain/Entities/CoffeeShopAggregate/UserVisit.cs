using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

using BusinessConstants = Domain.BusinessConstants;

/// <summary>
/// Агрегированная информация о посещениях пользователем конкретной кофейни.
/// Создаётся автоматически при первом CheckIn и обновляется при последующих.
/// </summary>
public sealed class UserVisit : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid ShopId { get; private set; }
    
    /// <summary>
    /// Дата первого посещения (первый CheckIn)
    /// </summary>
    public DateTime FirstVisitedAt { get; private set; }
    
    /// <summary>
    /// Дата последнего посещения (последний CheckIn)
    /// </summary>
    public DateTime LastVisitedAt { get; private set; }
    
    /// <summary>
    /// Общее количество CheckIn'ов в этой кофейне
    /// </summary>
    public int VisitCount { get; private set; }
    
    /// <summary>
    /// Есть ли у пользователя отзыв на эту кофейню
    /// </summary>
    public bool HasReview { get; private set; }
    
    /// <summary>
    /// Заметка о посещении (опционально)
    /// </summary>
    public string? Note { get; private set; }
    
    // БЕЗ навигационного свойства к CoffeeShop!
    
    private UserVisit() { } // Для EF Core

    private UserVisit(Guid userId, Guid shopId, DateTime visitedAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ShopId = shopId;
        FirstVisitedAt = visitedAt;
        LastVisitedAt = visitedAt;
        VisitCount = 1;
        HasReview = false;
    }

    /// <summary>
    /// Создать запись о первом посещении
    /// </summary>
    public static UserVisit CreateFirstVisit(Guid userId, Guid shopId, DateTime visitedAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");
        
        if (shopId == Guid.Empty)
            throw new DomainException("ShopId cannot be empty");

        return new UserVisit(userId, shopId, visitedAt);
    }
    
    /// <summary>
    /// Зарегистрировать новое посещение
    /// </summary>
    public void RegisterVisit(DateTime visitedAt)
    {
        LastVisitedAt = visitedAt;
        VisitCount++;
    }
    
    /// <summary>
    /// Отметить что пользователь оставил отзыв
    /// </summary>
    public void MarkAsReviewed()
    {
        HasReview = true;
    }
    
    /// <summary>
    /// Добавить/обновить заметку о месте
    /// </summary>
    public void UpdateNote(string? note)
    {
        if (note?.Length > BusinessConstants.MaxVisitNoteLength)
            throw new DomainException(
                $"Note cannot be longer than {BusinessConstants.MaxVisitNoteLength} characters");
        
        Note = note;
    }
}