using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Moderation.Application.Requests;

public class LoginRequest : IRequest<Response<LoginResponse>>
{
    public string Email { get; set; }
    public string Password { get; set; }
}