using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Moderation.Application.Features.Admin.Audit;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public static class UpdateModerationCoffeeShopStatusHandler
{
    public static async Task<(Response, object?)> Handle(
        UpdateModerationCoffeeShopStatusCommand command,
        IModerationShopRepository repository,
        IModerationAuditLogRepository auditLogRepository,
        IMapper mapper,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.Id, ct);

        if (shop == null)
            return (Response.Error("CoffeeShop not found"), null);

        object? outboundEvent = null;
        string? auditComment = null;
        ModerationAuditAction? auditAction = null;

        if (command.ModerationStatus == ModerationStatus.Approved)
        {
            shop.Approve();
            auditAction = ModerationAuditAction.Approved;
            
            outboundEvent = new ModerationShopApprovedEvent(
                shop.UserId,
                mapper.Map<ShopDto>(shop));
        }
        else if (command.ModerationStatus == ModerationStatus.Rejected)
        {
            var rejectReason = string.IsNullOrWhiteSpace(command.Comment)
                ? "Rejected by moderator"
                : command.Comment.Trim();
            shop.Reject(rejectReason);
            auditAction = ModerationAuditAction.Rejected;
            auditComment = rejectReason;
        }

        if (auditAction.HasValue)
        {
            await ModerationAuditWriter.WriteAsync(
                auditLogRepository,
                ModerationAuditEntityType.Shop,
                shop.Id,
                shop.Name,
                auditAction.Value,
                command.UserId,
                auditComment,
                ct);
        }

        return (Response.Success(), outboundEvent);
    }
}