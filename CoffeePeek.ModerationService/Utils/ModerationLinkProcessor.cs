using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.ModerationService.Utils;

public static class ModerationLinkProcessor
{
    public static void ProcessLinks<TEntity>(
        DbSet<TEntity> dbSet,
        Guid shopId,
        IReadOnlyCollection<Guid>? relatedIds,
        Func<Guid, Guid, TEntity> linkFactory,
        ILogger logger,
        string relationName)
        where TEntity : class
    {
        if (relatedIds is null || relatedIds.Count == 0)
        {
            logger.LogDebug(
                "No {RelationName} links to process for ModerationShop '{ShopId}'.",
                relationName,
                shopId);
            return;
        }

        foreach (var relatedId in relatedIds)
        {
            var link = linkFactory(shopId, relatedId);
            dbSet.Add(link);
        }

        logger.LogInformation(
            "{Count} {RelationName} links processed for ModerationShop '{ShopId}'.",
            relatedIds.Count,
            relationName,
            shopId);
    }
}
