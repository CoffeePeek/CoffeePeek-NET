using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class PasswordResetRequestedEventHandler(
    IResend resend,
    IConfiguration config,
    IEmailTemplateService templateService,
    ILogger<PasswordResetRequestedEventHandler> logger)
{
    public async Task Handle(PasswordResetRequestedInternalEvent @event)
    {
        var resetUrl =
            $"{config["WebClientUrl"]}/reset-password?token={Uri.EscapeDataString(@event.ResetToken)}";

        var message = new EmailMessage
        {
            From = "CoffeePeek.by <info@coffeepeek.by>",
            To = @event.Email,
            Subject = "Reset your CoffeePeek password ☕",
            HtmlBody = templateService.GetPasswordResetHtml(@event.Username, resetUrl)
        };

        try
        {
            await resend.EmailSendAsync(message);
            logger.LogInformation("Password reset email sent to {Email}", @event.Email);
        }
        catch (ResendException e)
        {
            logger.LogError(e, "Resend error for password reset to {Email}", @event.Email);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error sending password reset email to {Email}", @event.Email);
            throw;
        }
    }
}
