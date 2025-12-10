using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.User;

public class GetProfileRequest(Guid userId) : IRequest<Response<UserDto>>
{
    public Guid UserId { get;} = userId;
}