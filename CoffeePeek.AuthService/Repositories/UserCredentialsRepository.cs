using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.AuthService.Repositories;

public class UserCredentialsRepository(
    IGenericRepository<UserCredentials> userRepository,
    IGenericRepository<OutboxEvent> outboxRepository) : IUserCredentialsRepository
{
    public async Task AddAsync(UserCredentials user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await userRepository.AddAsync(user, ct);
    }

    public async Task<bool> UserExists(string email, CancellationToken ct = default)
    {
        return await userRepository.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<UserCredentials?> GetByIdAsync(Guid userId)
    {
        return await userRepository.Query()
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
            .SingleOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<UserCredentials?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await userRepository.QueryAsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UserCredentials?> GetByProviderAsync(string provider, string providerId, CancellationToken ct = default)
    {
        return await userRepository.FirstOrDefaultAsync(
            u => u.OAuthProvider == provider && u.ProviderId == providerId, ct);
    }

    public Task UpdateAsync(UserCredentials user, CancellationToken ct = default)
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