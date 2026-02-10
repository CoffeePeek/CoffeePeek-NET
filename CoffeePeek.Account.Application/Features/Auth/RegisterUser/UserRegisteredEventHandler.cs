using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public class UserRegisteredEventHandler
{
    public async Task Handle(
        UserRegisteredInternalEvent @event,
        IResend resend,
        IConfiguration config,
        IEmailTemplateService templateService,
        ILogger<UserRegisteredEventHandler> logger,
        CancellationToken ct)
    {
        var confirmationUrl = $"{config["WebClientUrl"]}/confirm-email?token={@event.ConfirmationToken}";

        var message = new EmailMessage
        {
            From = "CoffeePeek.by <info@coffeepeek.by>",
            To = @event.Email,
            Subject = "Perfectly roasted beans are waiting for you! ☕",
            HtmlBody = templateService.GetConfirmationHtml(@event.Username, confirmationUrl)
        };

        try
        {
            await resend.EmailSendAsync(message, ct);
        }
        catch (ResendException e)
        {
            logger.LogError(e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
        
        logger.LogInformation("Confirmation email sent to {Email}", @event.Email);
    }
}