using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Domain.Repositories;

public interface IUserCredentialsRepository
{
    Task AddAsync(UserCredential user, CancellationToken cancellationToken = default);
    Task<bool> UserExists(string email, CancellationToken cancellationToken = default);
    Task<UserCredential?> GetByIdAsync(Guid userId);
    Task<UserCredential?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserCredential?> GetByProviderAsync(string provider, string providerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCredential user, CancellationToken ct = default);
    
    Task AddOutboxEventAsync(OutboxEvent @event, CancellationToken ct);
    Task<List<OutboxEvent>> GetUnprocessedOutboxEvents(CancellationToken ct = default);
    Task MarkOutboxEventAsProcessed(Guid eventId, CancellationToken ct = default);
}