using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Account.Application.Features.Admin.Users;

public record GetAdminUsersQuery(
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20,
    [StringLength(100)] string? Search = null,
    [StringLength(50)] string? Role = null);
