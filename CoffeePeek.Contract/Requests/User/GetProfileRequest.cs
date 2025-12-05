using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Response;
using MediatR;

namespace CoffeePeek.Contract.Requests.User;

public class GetProfileRequest(Guid userId) : IRequest<Response<UserDto>>
{
    public Guid UserId { get;} = userId;
}