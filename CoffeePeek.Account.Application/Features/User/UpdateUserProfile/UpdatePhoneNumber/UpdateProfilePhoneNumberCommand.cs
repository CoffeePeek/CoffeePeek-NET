using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdatePhoneNumber;

public record UpdateProfilePhoneNumberCommand(
    [property: JsonIgnore] Guid UserId, 
    string PhoneNumber)
    : IRequest<UpdateEntityResponse<string>>;
