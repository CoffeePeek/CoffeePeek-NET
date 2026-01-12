using MediatR;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}