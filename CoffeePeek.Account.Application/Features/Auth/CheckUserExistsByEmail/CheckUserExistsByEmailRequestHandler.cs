using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;

public class CheckUserExistsByEmailRequestHandler(IUserRepository userRepository, EmailExistenceFilter emailExistenceFilter)
    : IRequestHandler<CheckUserExistsByEmailCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(CheckUserExistsByEmailCommand request, CancellationToken cancellationToken)
    {
        if (emailExistenceFilter.MightExist(request.Email))
        {
            return Response<bool>.Success(true, "Email exists");
        }
        
        var userExists = await userRepository.UserExistsByEmail(request.Email, cancellationToken);

        return Response<bool>.Success(userExists, userExists ? "Email exists" : "Email does not exist");
    }
}