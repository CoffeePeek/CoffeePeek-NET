using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.User;

public class GetAllUsersRequest : IRequest<Response<UserDto[]>>;