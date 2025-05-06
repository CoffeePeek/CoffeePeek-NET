using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Moderation.Application.Requests;

public class RegisterUserRequest(string email, string password) : IRequest<Response<RegisterUserResponse>>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}