using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.Contract.Requests.Auth;

public class RegisterUserRequest(string userName, string email, string password, bool isAdmin = false)
    : IRequest<Response<RegisterUserResponse>>
{
    [Required] public string UserName { get; set; } = userName;
    [Required] public string Email { get; } = email;
    [Required] public string Password { get; } = password;
    public bool IsAdmin { get; set; } = isAdmin;
}