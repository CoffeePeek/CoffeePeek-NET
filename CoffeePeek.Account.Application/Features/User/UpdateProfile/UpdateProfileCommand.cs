using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateProfile;

public record UpdateProfileCommand : IRequest<Response<UpdateProfileResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? About { get; set; }
}