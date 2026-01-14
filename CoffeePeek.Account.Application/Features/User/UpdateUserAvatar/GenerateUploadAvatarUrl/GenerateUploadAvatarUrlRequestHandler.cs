using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserAvatar.GenerateUploadAvatarUrl;

public class GenerateUploadAvatarUrlRequestHandler(IStorageService storageService)
    : IRequestHandler<GenerateUploadAvatarUrlCommand, Response<GenerateUploadUrlResponse>>
{
    public async Task<Response<GenerateUploadUrlResponse>> Handle(GenerateUploadAvatarUrlCommand request,
        CancellationToken cancellationToken)
    {
        var (uploadUrl, storageKey) =
            await storageService.GetPresignedUploadUrlAsync(request.Request.FileName, request.Request.ContentType);

        return Response<GenerateUploadUrlResponse>.Success(new GenerateUploadUrlResponse(uploadUrl, storageKey));
    }
}