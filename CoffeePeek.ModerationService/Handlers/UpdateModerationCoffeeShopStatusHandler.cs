using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Outbox;
using MapsterMapper;
using MediatR;
using System.Text.Json;
using System.IO;
using MassTransit;
using Response = CoffeePeek.Contract.Responses.Response;

namespace CoffeePeek.ModerationService.Handlers;

public class UpdateModerationCoffeeShopStatusHandler(
    IModerationShopRepository repository,
    IOutboxEventPublisher outboxEventPublisher,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateModerationCoffeeShopStatusHandler> logger,
    IPublishEndpoint publishEndpoint) 
    : IRequestHandler<UpdateModerationCoffeeShopStatusRequest, Response>
{
    private const string AgentLogPath = @"c:\Users\User\RiderProjects\CoffeePeek-BackEnd\.cursor\debug.log";

    private static void WriteAgentLog(object payload)
    {
        try
        {
            var directory = Path.GetDirectoryName(AgentLogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(AgentLogPath, JsonSerializer.Serialize(payload) + Environment.NewLine);
        }
        catch
        {
            // swallow logging errors to avoid impacting test flow
        }
    }

    public async Task<Response> Handle(UpdateModerationCoffeeShopStatusRequest request, CancellationToken cancellationToken)
    {
        // #region agent log
        WriteAgentLog(new
        {
            sessionId = "debug-session",
            runId = "pre-fix",
            hypothesisId = "H1",
            location = "UpdateModerationCoffeeShopStatusHandler.Handle",
            message = "enter handler",
            data = new { request.Id, request.ModerationStatus, request.UserId },
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        // #endregion

        var shop = await repository.GetByIdAsync(request.Id);

        if (shop == null)
        {
            logger.LogWarning("CoffeeShop with ID {CoffeeShopId} not found for moderation status update.", request.Id);
            // #region agent log
            WriteAgentLog(new
            {
                sessionId = "debug-session",
                runId = "pre-fix",
                hypothesisId = "H3",
                location = "UpdateModerationCoffeeShopStatusHandler.Handle",
                message = "shop not found",
                data = new { request.Id },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            // #endregion
            throw new NotFoundException("CoffeeShop not found");
        }
        
        shop.ModerationStatus = request.ModerationStatus;
        logger.LogInformation("Updating moderation status for CoffeeShop {CoffeeShopId} to {NewStatus}.", request.Id, request.ModerationStatus);

        await repository.UpdateAsync(shop);

        if (request.ModerationStatus == ModerationStatus.Approved)
        {
            // #region agent log
            WriteAgentLog(new
            {
                sessionId = "debug-session",
                runId = "pre-fix",
                hypothesisId = "H1",
                location = "UpdateModerationCoffeeShopStatusHandler.Handle",
                message = "publishing approved event",
                data = new
                {
                    request.Id,
                    shop.UserId,
                    shop.ModerationStatus,
                    hasLocation = shop.Location != null,
                    hasContact = shop.ShopContacts != null,
                    photoCount = shop.ShopPhotos?.Count
                },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            // #endregion

            var approvedEvent = new CoffeeShopApprovedEvent(request.UserId, mapper.Map<ShopDto>(shop));

            await outboxEventPublisher.PublishAsync(approvedEvent, cancellationToken);
            logger.LogInformation("Added CoffeeShopApprovedEvent to outbox for CoffeeShop {CoffeeShopId}.", request.Id);

            // #region agent log
            WriteAgentLog(new
            {
                sessionId = "debug-session",
                runId = "pre-fix",
                hypothesisId = "H1",
                location = "UpdateModerationCoffeeShopStatusHandler.Handle",
                message = "publishing to IPublishEndpoint",
                data = new
                {
                    request.Id,
                    request.UserId
                },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            // #endregion

            await publishEndpoint.Publish(approvedEvent, cancellationToken);
            logger.LogInformation("Published CoffeeShopApprovedEvent via IPublishEndpoint for CoffeeShop {CoffeeShopId}.", request.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // #region agent log
        WriteAgentLog(new
        {
            sessionId = "debug-session",
            runId = "pre-fix",
            hypothesisId = "H2",
            location = "UpdateModerationCoffeeShopStatusHandler.Handle",
            message = "status updated and saved",
            data = new { shop.Id, shop.ModerationStatus },
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        // #endregion
        
        return Response.Success();
    }
}
