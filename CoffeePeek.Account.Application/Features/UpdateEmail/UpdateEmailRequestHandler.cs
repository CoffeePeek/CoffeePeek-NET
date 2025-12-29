using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.UpdateEmail;

public class UpdateEmailRequestHandler(
    IUserRepository userRepository, 
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEmailCommand, UpdateEntityResponse<string>>
{
    public async Task<UpdateEntityResponse<string>> Handle(UpdateEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.UpdateEmail(request.Email);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateEntityResponse<string>.Success(user.Email, "Email updated successfully");
    }
}