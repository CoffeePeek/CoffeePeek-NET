using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Shop.GenerateUploadUrl;

public record GenerateUploadUrlsCommand(IList<UploadUrlRequest> Requests)
    : IRequest<Response<List<GenerateUploadUrlResponse>>>;