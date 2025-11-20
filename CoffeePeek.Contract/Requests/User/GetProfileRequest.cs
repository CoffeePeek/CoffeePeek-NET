using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Response;
using MediatR;

namespace CoffeePeek.Contract.Requests.User;

public class GetProfileRequest(int userId) : IRequest<Response<UserDto>>
{
    public int UserId { get;} = userId;
}