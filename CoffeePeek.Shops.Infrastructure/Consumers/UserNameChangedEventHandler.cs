using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class UserNameChangedHandler(ShopsDbContext dbContext)
{
    public async Task Handle(UserNameChangedEvent message, CancellationToken cancellationToken = default)
    {
        // ExecuteUpdateAsync bypasses EF Core change tracker (skips AuditInterceptor.UpdatedAtUtc). Acceptable: UserName on Review is a denormalized display field, not auditable content.
        await dbContext.Reviews
            .Where(r => r.UserId == message.UserId && r.UserName != message.NewUserName)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.UserName, message.NewUserName), cancellationToken);
    }
}
