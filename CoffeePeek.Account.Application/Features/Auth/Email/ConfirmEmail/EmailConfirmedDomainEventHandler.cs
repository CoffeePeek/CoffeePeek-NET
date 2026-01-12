using CoffeePeek.Account.Domain.Events;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;

public class EmailConfirmedDomainEventHandler : INotificationHandler<EmailConfirmedInternalEvent>
{
    public Task Handle(EmailConfirmedInternalEvent notification, CancellationToken cancellationToken)
    {
        //todo add logic implementation

        return Task.CompletedTask;
    }
}