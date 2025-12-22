using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public class LogoutCommand : IRequest<Response>
{
    public Guid UserId { get; set; }
}