using CoffeePeek.Account.Application.Features.User.Email.ConfirmEmail;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;

public class ConfirmEmailHandler(IUserCredentialsRepository userRepository, IUnitOfWork unitOfWork) 
    : IRequestHandler<ConfirmEmailCommand, Response>
{
    public async Task<Response> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
         var user = await userRepository.GetByEmailConfirmToken(request.Token, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found.");

        if (user.EmailConfirmed)
            throw new BusinessException("Email already confirmed.");

        user.ConfirmEmail(request.Token);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success(new { message = "Email confirmed successfully!" });
    }
}