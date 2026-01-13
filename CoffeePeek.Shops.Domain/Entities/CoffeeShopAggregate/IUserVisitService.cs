using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

/// <summary>
/// Domain Service для управления посещениями кофеен.
/// Синхронизирует CheckIn'ы с агрегированными данными в UserVisit.
/// </summary>
public interface IUserVisitService
{
    /// <summary>
    /// Зарегистрировать посещение (вызывается при CheckIn)
    /// </summary>
    Task<Result<Guid>> RegisterVisitAsync(
        Guid userId,
        Guid shopId,
        DateTime visitedAt,
        bool hasReview = false,
        CancellationToken ct = default);

    /// <summary>
    /// Обновить информацию о наличии отзыва
    /// </summary>
    Task<Result> UpdateReviewStatusAsync(
        Guid userId,
        Guid shopId,
        bool hasReview,
        CancellationToken ct = default);

    /// <summary>
    /// Получить список посещённых кофеен
    /// </summary>
    Task<List<CoffeeShop>> GetVisitedShopsAsync(
        Guid userId,
        VisitedSortOrder sortOrder = VisitedSortOrder.LastVisited,
        CancellationToken ct = default);

    /// <summary>
    /// Получить статистику посещений пользователя
    /// </summary>
    Task<UserVisitStatistics> GetVisitStatisticsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Проверить посещал ли пользователь кофейню
    /// </summary>
    Task<bool> HasVisitedAsync(Guid userId, Guid shopId, CancellationToken ct = default);
}

/// <summary>
/// Порядок сортировки посещённых мест
/// </summary>
public enum VisitedSortOrder
{
    FirstVisited,
    LastVisited,
    MostVisited,
    Alphabetical
}

/// <summary>
/// Статистика посещений пользователя
/// </summary>
public record UserVisitStatistics
{
    /// <summary>
    /// Количество уникальных посещённых кофеен
    /// </summary>
    public int UniqueShopsVisited { get; init; }

    /// <summary>
    /// Общее количество CheckIn'ов
    /// </summary>
    public int TotalCheckIns { get; init; }

    /// <summary>
    /// Количество кофеен с отзывами
    /// </summary>
    public int ShopsWithReviews { get; init; }

    /// <summary>
    /// Любимая кофейня (самая посещаемая)
    /// </summary>
    public Guid? FavoriteShopId { get; init; }

    /// <summary>
    /// Количество посещений любимой кофейни
    /// </summary>
    public int FavoriteShopVisitCount { get; init; }
}