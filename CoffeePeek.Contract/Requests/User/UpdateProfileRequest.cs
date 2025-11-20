using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.User;
using MediatR;

namespace CoffeePeek.Contract.Requests.User;

public class UpdateProfileRequest : IRequest<Response.Response<UpdateProfileResponse>>
{
    [JsonIgnore] public int UserId { get; set; }

    [JsonIgnore] public string PhotoUrl { get; set; }
    public string UserName { get; set; }
    public string About { get; set; }
}