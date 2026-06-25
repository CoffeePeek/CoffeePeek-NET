using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class CommunityCommentRepository(ShopsDbContext dbContext) : ICommunityCommentRepository
{
    public void Add(CommunityComment comment) => dbContext.CommunityComments.Add(comment);

    public Task<CommunityComment?> GetById(Guid commentId, CancellationToken ct = default) =>
        dbContext.CommunityComments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
}
