namespace CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;

public record CreateCheckInResponse(Guid CheckInId, Guid? ReviewId = null);