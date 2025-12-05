using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Repositories;

public interface IUserCredentialsRepository
{
    Task AddAsync(UserCredentials user, CancellationToken cancellationToken = default);
    Task<bool> UserExists(string email, CancellationToken cancellationToken = default);
    Task<UserCredentials?> GetByIdAsync(Guid userId);
    Task<UserCredentials?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserCredentials?> GetByProviderAsync(string provider, string providerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCredentials user, CancellationToken ct = default);
    
    Task AddOutboxEventAsync(OutboxEvent @event, CancellationToken ct);
    Task<List<OutboxEvent>> GetUnprocessedOutboxEvents(CancellationToken ct = default);
    Task MarkOutboxEventAsProcessed(Guid eventId, CancellationToken ct = default);
}