using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Shop.GenerateUploadUrl;

public class GenerateUploadUrlHandler(IStorageService storageService)
    : IRequestHandler<GenerateUploadUrlsCommand, Response<List<GenerateUploadUrlResponse>>>
{
    public async Task<Response<List<GenerateUploadUrlResponse>>> Handle(
        GenerateUploadUrlsCommand command, 
        CancellationToken ct)
    {
        var results = new List<GenerateUploadUrlResponse>();

        foreach (var req in command.Requests)
        {
            var (url, key) = await storageService.GetPresignedUploadUrlAsync(req.FileName, req.ContentType);
            results.Add(new GenerateUploadUrlResponse(url, key));
        }

        return Response<List<GenerateUploadUrlResponse>>.Success(results);
    }
}