using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.UserService.Models;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Handlers;

public class GetAllUsersHandler(IGenericRepository<User> userRepository, IMapper mapper) 
    : IRequestHandler<GetAllUsersRequest, Response<UserDto[]>>
{
    public async Task<Response<UserDto[]>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await userRepository
            .QueryAsNoTracking()
            .ProjectToType<UserDto>(mapper.Config)
            .ToArrayAsync(cancellationToken);
        
        return Response<UserDto[]>.Success(users);
    }
}