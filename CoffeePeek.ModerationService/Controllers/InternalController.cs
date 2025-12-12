using CoffeePeek.ModerationService.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.ModerationService.Controllers;

[ApiController]
[Route("api/internal")]
public class InternalController(IModerationShopRepository repository) : ControllerBase
{
    [Authorize]
    [HttpGet("statistics/{userId:guid}/approved-shops-count")]
    public async Task<IActionResult> GetApprovedShopsCount(Guid userId)
    {
        var count = await repository.GetApprovedShopsCountByUserIdAsync(userId);
        return Ok(new { Count = count });
    }
}
