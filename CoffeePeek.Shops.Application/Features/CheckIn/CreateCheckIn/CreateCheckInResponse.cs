namespace CoffeePeek.Contract.Responses.CoffeeShop;

public record CreateCheckInResponse(Guid CheckInId, Guid? ReviewId = null);