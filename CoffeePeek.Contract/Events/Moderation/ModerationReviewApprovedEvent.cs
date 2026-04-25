using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Contract.Events.Moderation;

public record ModerationReviewApprovedEvent(ModerationReviewDto Review);