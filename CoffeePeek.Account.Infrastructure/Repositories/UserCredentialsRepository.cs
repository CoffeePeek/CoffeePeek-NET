using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Auth.Infrastructure.Repositories;

public class UserCredentialsRepository(
    IGenericRepository<UserCredential> userRepository,
    IGenericRepository<OutboxEvent> outboxRepository) : IUserCredentialsRepository
{
    public async Task AddAsync(UserCredential user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await userRepository.AddAsync(user, ct);
    }

    public async Task<bool> UserExists(string email, CancellationToken ct = default)
    {
        return await userRepository.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<UserCredential?> GetByIdAsync(Guid userId)
    {
        return await userRepository.Query()
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<UserCredential?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await userRepository.Query()
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UserCredential?> GetByProviderAsync(string provider, string providerId, CancellationToken ct = default)
    {
        return await userRepository.FirstOrDefaultAsync(
            u => u.OAuthProvider == provider && u.ProviderId == providerId, ct);
    }

    public Task UpdateAsync(UserCredential user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        userRepository.Update(user);
        return Task.CompletedTask;
    }

    public async Task AddOutboxEventAsync(OutboxEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        await outboxRepository.AddAsync(@event, ct);
    }

    public async Task<List<OutboxEvent>> GetUnprocessedOutboxEvents(CancellationToken ct = default)
    {
        return await outboxRepository.Query()
            .Where(e => !e.Processed)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task MarkOutboxEventAsProcessed(Guid eventId, CancellationToken ct = default)
    {
        var @event = await outboxRepository.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (@event != null)
        {
            @event.Processed = true;
            outboxRepository.Update(@event);
        }
    }
}