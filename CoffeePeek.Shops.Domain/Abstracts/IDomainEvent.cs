using MediatR;

namespace CoffeePeek.Shops.Domain.Abstracts;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}