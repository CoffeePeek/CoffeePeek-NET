using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateEmail;

public record UpdateProfileEmailCommand([property: JsonIgnore] Guid UserId, string Email)
    : IRequest<UpdateEntityResponse<string>>;