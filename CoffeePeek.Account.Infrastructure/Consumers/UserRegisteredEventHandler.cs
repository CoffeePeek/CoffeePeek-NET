using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class UserRegisteredEventHandler(
    IResend resend,
    IConfiguration config,
    IEmailTemplateService templateService,
    ILogger<UserRegisteredEventHandler> logger)
{
    public async Task Handle(UserRegisteredInternalEvent @event)
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
            await resend.EmailSendAsync(message);
            logger.LogInformation("Confirmation email sent to {Email}", @event.Email);
        }
        catch (ResendException e)
        {
            logger.LogError(e, "Resend error for {Email}", @event.Email);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error sending email to {Email}", @event.Email);
            throw;
        }
    }
}