using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Events;
using MediatR;
using Microsoft.Extensions.Configuration;
using Resend;

namespace CoffeePeek.Account.Application.Features.RegisterUser;

public class UserRegisteredEventHandler(
    IResend resend,
    IConfiguration config,
    IEmailTemplateService templateService) : INotificationHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        var confirmationUrl = $"{config["WebClientUrl"]}/confirm-email?token={notification.ConfirmationToken}";

        var message = new EmailMessage
        {
            From = "CoffeePeek <hello@resend.dev>",
            To = notification.Email,
            Subject = "Perfectly roasted beans are waiting for you! ☕",
            HtmlBody = templateService.GetConfirmationHtml(notification.Username, confirmationUrl)
        };

        await resend.EmailSendAsync(message, cancellationToken);
    }
}