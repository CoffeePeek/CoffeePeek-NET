using CoffeePeek.Contract.Response;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public class LogoutCommand : IRequest<Response>
{
    public Guid UserId { get; set; }
}