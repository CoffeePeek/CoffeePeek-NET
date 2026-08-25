using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoffeePeek.AccountService.Realtime;

[Authorize]
public class SessionHub : Hub<ISessionHubClient>;
