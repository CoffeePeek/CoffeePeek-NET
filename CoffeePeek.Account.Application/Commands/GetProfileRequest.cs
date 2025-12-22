using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public class GetProfileRequest(Guid userId) : IRequest<Response<UserDto>>
{
    public Guid UserId { get;} = userId;
}