using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.CheckUserExistsByEmail;

public class CheckUserExistsByEmailRequestHandler(IUserCredentialsRepository userCredentialsRepository)
    : IRequestHandler<CheckUserExistsByEmailCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(CheckUserExistsByEmailCommand request, CancellationToken cancellationToken)
    {
        var userExists = await userCredentialsRepository.UserExists(request.Email, cancellationToken);

        return Response<bool>.Success(userExists);
    }
}