using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using MediatR;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;

namespace CoffeePeek.Contract.Requests.User;

public record UpdateProfileRequest : IRequest<Response<UpdateProfileResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; }

    [JsonIgnore] public string PhotoUrl { get; set; }
    public string UserName { get; set; }
    public string About { get; set; }
    
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    public string Email { get; set; }
}