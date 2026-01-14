using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Requests;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserAvatar.GenerateUploadAvatarUrl;

public record GenerateUploadAvatarUrlCommand(UploadUrlRequest Request) : IRequest<Response<GenerateUploadUrlResponse>>;