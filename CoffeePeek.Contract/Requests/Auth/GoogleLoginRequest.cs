using CoffeePeek.Contract.Response.Auth;
using MediatR;

namespace CoffeePeek.Contract.Requests.Auth;

public class GoogleLoginRequest : IRequest<Response.Response<GoogleLoginResponse>>
{
    public string IdToken { get; set; }
}