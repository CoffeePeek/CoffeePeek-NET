using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record GetAdminUsersQuery(
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20,
    string? Search = null,
    string? Role = null);

public record GetAdminUsersResponse(
    IReadOnlyList<AdminUserResponse> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);
