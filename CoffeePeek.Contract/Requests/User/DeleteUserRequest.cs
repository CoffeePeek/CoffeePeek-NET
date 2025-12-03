using CoffeePeek.Contract.Response;
using MediatR;

namespace CoffeePeek.Contract.Requests.User;

public class DeleteUserRequest : IRequest<Response<bool>>
{
    public Guid UserId { get; }

    public DeleteUserRequest(Guid userId)
    {
        UserId = userId;
    }
}