namespace CoffeePeek.Account.Application.Features.Admin.ChangeRole;

public record ChangeRoleCommand(Guid UserId, Guid UserIdOfChange, Guid RoleId);