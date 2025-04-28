using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class GetAllUserRequestHandler(IUnitOfWork<CoffeePeekDbContext> unitOfWork, 
    IMapper mapper)
    : IRequestHandler<GetAllUsersRequest, Response<UserDto[]>>
{
    public async Task<Response<UserDto[]>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<User>();

        var usersQuery = repository.GetAll();
        
        var users = await usersQuery.ToListAsync(cancellationToken);
        
        var result = mapper.Map<UserDto[]>(users);
        
        return Response.SuccessResponse<Response<UserDto[]>>(result);
    }
}