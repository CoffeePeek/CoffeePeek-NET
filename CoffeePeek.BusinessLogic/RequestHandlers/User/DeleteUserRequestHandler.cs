using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class DeleteUserRequestHandler(IUnitOfWork<CoffeePeekDbContext> unitOfWork)
    : IRequestHandler<DeleteUserRequest, Response<bool>>
{
    public async Task<Response<bool>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.DbContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Response.ErrorResponse<Response<bool>>("User not found");
        }
        
        user.IsSoftDeleted = true;
        
        await unitOfWork.DbContext.SaveChangesAsync(cancellationToken);

        return Response.SuccessResponse<Response<bool>>(true);
    }
}