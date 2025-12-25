using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.UpdateProfile;

public record UpdateProfileCommand : IRequest<Response<UpdateProfileResponse>>
{
    public Guid UserId { get; set; }

    public string PhotoUrl { get; set; }
    public string UserName { get; set; }
    public string About { get; set; }
    
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    public string Email { get; set; }
}