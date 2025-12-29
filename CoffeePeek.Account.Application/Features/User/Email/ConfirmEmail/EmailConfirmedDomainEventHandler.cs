using CoffeePeek.Account.Domain.Events;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.Email.ConfirmEmail;

public class EmailConfirmedDomainEventHandler : INotificationHandler<EmailConfirmedDomainEvent>
{
    public Task Handle(EmailConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        //todo add logic implementation

        return Task.CompletedTask;
    }
}