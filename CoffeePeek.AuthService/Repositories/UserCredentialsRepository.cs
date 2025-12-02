using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.AuthService.Repositories;

public class UserCredentialsRepository(AuthDbContext context) : IUserCredentialsRepository
{
    public async Task AddAsync(UserCredentials user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await context.Users.AddAsync(user, ct);
    }

    public async Task<bool> UserExists(string email, CancellationToken ct = default)
    {
        return await context.Users.AnyAsync(u => u.Email == email, ct);
    }

    public Task<UserCredentials?> GetByIdAsync(Guid userId)
    {
        return context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
            .SingleOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<UserCredentials?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UserCredentials?> GetByProviderAsync(string provider, string providerId, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.OAuthProvider == provider && u.ProviderId == providerId, ct);
    }

    public async Task UpdateAsync(UserCredentials user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Email = user.Email;
        user.PasswordHash = user.PasswordHash;
        user.OAuthProvider = user.OAuthProvider;
        user.ProviderId = user.ProviderId;

        user.RefreshTokens = new HashSet<RefreshToken>(user.RefreshTokens);
        user.UserRoles = new HashSet<UserRole>(user.UserRoles);

    }

    public async Task AddOutboxEventAsync(OutboxEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await context.OutboxEvents.AddAsync(@event, ct);
    }

    public async Task<List<OutboxEvent>> GetUnprocessedOutboxEvents(CancellationToken ct = default)
    {
        return await context.OutboxEvents
            .Where(e => !e.Processed)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task MarkOutboxEventAsProcessed(Guid eventId, CancellationToken ct = default)
    {
        var @event = await context.OutboxEvents.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        @event?.Processed = true;
    }
}