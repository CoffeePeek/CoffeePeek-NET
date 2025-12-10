using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.Auth;

public class GoogleLoginRequest : IRequest<Response<GoogleLoginResponse>>
{
    public string IdToken { get; set; }
}