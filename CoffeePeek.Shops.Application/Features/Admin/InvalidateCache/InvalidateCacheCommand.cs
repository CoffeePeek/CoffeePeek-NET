using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Admin.InvalidateCache;

public record InvalidateCacheCommand(
    string? Category = null,
    bool InvalidateAll = false) : IRequest<Response<InvalidateCacheResponse>>;
