using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Shared.Extensions.Exceptions;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;

public class CheckUserExistsByEmailRequestHandler(IQueryUserRepository userRepository, EmailExistenceFilter emailExistenceFilter)
    : IRequestHandler<CheckUserExistsByEmailQuery, Response<bool>>
{
    public async Task<Response<bool>> Handle(CheckUserExistsByEmailQuery query, CancellationToken cancellationToken)
    {
        if (emailExistenceFilter.MightExist(query.Email))
        {
            return Response<bool>.Success(true, "Email exists");
        }
        
        var userExists = await userRepository.UserExistsByEmail(query.Email, cancellationToken);

        return userExists
            ? Response<bool>.Success(true, "Email exists")
            : throw new NotFoundException("Email does not exist");
    }
}