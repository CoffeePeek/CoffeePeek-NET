using System.Text.Json.Serialization;
using CoffeePeek.MediaService.Commands.Base;

namespace CoffeePeek.MediaService.Commands;

public record GenerateShopPhotosCommand(List<PhotoRequest> Requests, [property:JsonIgnore] Guid OwnerId);