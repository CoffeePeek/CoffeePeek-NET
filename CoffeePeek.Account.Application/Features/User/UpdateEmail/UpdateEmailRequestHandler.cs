using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.UpdateEmail;

public class UpdateEmailRequestHandler(
    IUserCredentialsRepository userRepository, 
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEmailCommand, UpdateEntityResponse<string>>
{
    public async Task<UpdateEntityResponse<string>> Handle(UpdateEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.UpdateEmail(request.Email);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateEntityResponse<string>.Success(user.Email, "Email updated successfully");
    }
}