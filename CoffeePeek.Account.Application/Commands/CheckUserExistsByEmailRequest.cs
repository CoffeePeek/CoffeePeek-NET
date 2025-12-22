using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public class CheckUserExistsByEmailRequest(string email) : IRequest<Response>
{
    [Required]
    public string Email { get; } = email;
}