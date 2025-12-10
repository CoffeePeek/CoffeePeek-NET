using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public class CheckUserExistsByEmailCommand(string email) : IRequest<Response<bool>>
{
    public string Email { get; init; }
}