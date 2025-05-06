using CoffeePeek.BusinessLogic.Exceptions;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class GetUserRequestHandler(IMapper mapper, 
    IUnitOfWork<CoffeePeekDbContext> unitOfWork)
    : IRequestHandler<GetUserRequest, Response<UserDto>>
{
    public async Task<Response<UserDto>> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.DbContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }
        
        var result = mapper.Map<UserDto>(user);
        
        return Response.SuccessResponse<Response<UserDto>>(result);
    }
}