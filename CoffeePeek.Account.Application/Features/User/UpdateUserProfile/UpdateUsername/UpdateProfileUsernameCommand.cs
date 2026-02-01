using CoffeePeek.Contract.Abstract;
using MediatR;
using Newtonsoft.Json;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;

public record UpdateProfileUsernameCommand(
    [property: JsonIgnore] Guid UserId, 
    string Username)
    : IRequest<UpdateEntityResponse<string>>;