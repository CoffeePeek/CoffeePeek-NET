namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record GetAdminUsersResponse(
    IReadOnlyList<AdminUserResponse> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);
