using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.Extensions.Configuration;
using Resend;

namespace CoffeePeek.Account.Application.Features.User.Email.ResendEmailConfirmation;

public class ResendEmailConfirmationHandler(
    IResend resend,
    IEmailTemplateService templateService,
    IUserCredentialsRepository userRepository,
    IUnitOfWork unitOfWork,
    IConfiguration config) : IRequestHandler<ResendEmailConfirmationCommand, Response>
{
    private const string WebClientUrl = nameof(WebClientUrl);
    
    public async Task<Response> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found");

        if (user.EmailConfirmed)
            throw new BusinessException("Email already confirmed.");

        var confirmationUrl = $"{config[WebClientUrl]}/confirm-email?token={user.EmailConfirmationToken}";

        var message = new EmailMessage
        {
            From = "CoffeePeek <hello@resend.dev>",
            To = user.Email,
            Subject = "Perfectly roasted beans are waiting for you! ☕",
            HtmlBody = templateService.GetConfirmationHtml(user.User!.Username, confirmationUrl)
        };

        
        await resend.EmailSendAsync(message, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success(new { message = "Email sent successfully!" });
    }
}