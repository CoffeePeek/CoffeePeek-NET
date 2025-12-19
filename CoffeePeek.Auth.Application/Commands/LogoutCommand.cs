using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public class LogoutCommand : IRequest<Response>
{
    public Guid UserId { get; set; }
}