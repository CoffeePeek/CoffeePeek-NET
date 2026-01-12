using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.GenerateUploadUrl;

public record GenerateUploadUrlsCommand(IList<UploadUrlRequest> Requests)
    : IRequest<Response<List<GenerateUploadUrlResponse>>>;