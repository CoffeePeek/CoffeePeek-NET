using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Configuration;
using Resend;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;

public class ResendEmailConfirmationHandler(
    IResend resend,
    IEmailTemplateService templateService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IConfiguration config) : IRequestHandler<ResendEmailConfirmationCommand, Response>
{
    private const string WebClientUrl = nameof(WebClientUrl);
    
    public async Task<Response> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found");

        if (user.Credentials.EmailConfirmed)
            throw new DomainException("Email already confirmed.");

        var confirmationUrl = $"{config[WebClientUrl]}/confirm-email?token={user.Credentials.EmailConfirmationToken}";

        var message = new EmailMessage
        {
            From = "CoffeePeek <hello@resend.dev>",
            To = user.Credentials.Email.Value,
            Subject = "Perfectly roasted beans are waiting for you! ☕",
            HtmlBody = templateService.GetConfirmationHtml(user.Username.Value, confirmationUrl)
        };

        
        await resend.EmailSendAsync(message, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success(new { message = "Email sent successfully!" });
    }
}