using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateProfile;

public record UpdateProfileCommand(Guid UserId, string? Username, string? PhoneNumber, string? About)
    : IRequest<Response<UpdateProfileResponse>>;