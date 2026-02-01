using CoffeePeek.Contract.Abstract;
using MediatR;
using Newtonsoft.Json;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAbout;

public record UpdateProfileAboutCommand(
    [property: JsonIgnore] Guid UserId,
    string About)
    : IRequest<UpdateEntityResponse<string>>;