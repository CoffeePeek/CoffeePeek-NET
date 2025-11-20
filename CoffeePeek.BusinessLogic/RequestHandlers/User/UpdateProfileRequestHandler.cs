using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.UnitOfWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class UpdateProfileRequestHandler(IUnitOfWork<CoffeePeekDbContext> unitOfWork)
    : IRequestHandler<UpdateProfileRequest, Response<UpdateProfileResponse>>
{
    public async Task<Response<UpdateProfileResponse>> Handle(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.DbContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Response.ErrorResponse<Response<UpdateProfileResponse>>("User not found");
        }

        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            user.UserName = request.UserName.Trim();
        }

        if (request.About is not null)
        {
            user.About = request.About;
        }

        await unitOfWork.DbContext.SaveChangesAsync(cancellationToken);

        return Response.SuccessResponse<Response<UpdateProfileResponse>>(new UpdateProfileResponse(), "Profile updated successfully");
    }
}