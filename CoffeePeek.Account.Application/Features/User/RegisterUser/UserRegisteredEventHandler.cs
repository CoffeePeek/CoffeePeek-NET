using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Events;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace CoffeePeek.Account.Application.Features.User.RegisterUser;

public class UserRegisteredEventHandler(
    IResend resend,
    IConfiguration config,
    IEmailTemplateService templateService,
    ILogger<UserRegisteredEventHandler> logger) : INotificationHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var confirmationUrl = $"{config["WebClientUrl"]}/confirm-email?token={notification.ConfirmationToken}";

            var message = new EmailMessage
            {
                From = "Coffeepeek.by <coffeepeek.by@coffeepeek.by>",
                To = notification.Email,
                Subject = "Perfectly roasted beans are waiting for you! ☕",
                HtmlBody = templateService.GetConfirmationHtml(notification.Username, confirmationUrl)
            };

            await resend.EmailSendAsync(message, cancellationToken);
        }
        catch (ResendException e)
        {
            logger.LogError(e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}