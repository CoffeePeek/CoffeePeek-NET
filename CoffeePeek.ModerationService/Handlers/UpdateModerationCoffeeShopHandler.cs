using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Entities;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using CoffeePeek.ModerationService.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ModerationService.Handlers;

public class UpdateModerationCoffeeShopHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
    ModerationDbContext dbContext,
    IYandexGeocodingService geocodingService) 
    : IRequestHandler<UpdateModerationCoffeeShopRequest, Response<UpdateModerationCoffeeShopResponse>>
{
    public async Task<Response<UpdateModerationCoffeeShopResponse>> Handle(UpdateModerationCoffeeShopRequest request,
        CancellationToken cancellationToken)
    {
        var shop = await repository.GetByIdAsync(request.ReviewShopId);

        if (shop == null || shop.UserId != request.UserId)
        {
            return Response<UpdateModerationCoffeeShopResponse>.Error("Moderation CoffeeShop not found");
        }

        // Update basic shop properties
        if (request.Name != null)
        {
            shop.Name = request.Name;
        }
        if (request.Description != null)
        {
            shop.Description = request.Description;
        }
        if (request.PriceRange.HasValue)
        {
            shop.PriceRange = request.PriceRange.Value;
        }
        if (request.CityId.HasValue)
        {
            shop.CityId = request.CityId.Value;
        }
        if (request.NotValidatedAddress != null)
        {
            shop.NotValidatedAddress = request.NotValidatedAddress;
            // Attempt geocoding for new address
            var geocodingResult = await geocodingService.GeocodeAsync(request.NotValidatedAddress, cancellationToken);
            if (geocodingResult != null)
            {
                shop.IsAddressValidated = true;
                shop.Latitude = geocodingResult.Latitude;
                shop.Longitude = geocodingResult.Longitude;
                shop.Address = request.NotValidatedAddress;
            }
        }

        // Update or create ShopContacts
        if (request.ShopContact != null)
        {
            if (shop.ShopContactId.HasValue)
            {
                var existingContact = await dbContext.ShopContacts.FindAsync([shop.ShopContactId.Value], cancellationToken);
                if (existingContact != null)
                {
                    existingContact.PhoneNumber = request.ShopContact.PhoneNumber;
                    existingContact.InstagramLink = request.ShopContact.InstagramLink;
                    existingContact.Email = request.ShopContact.Email;
                    existingContact.SiteLink = request.ShopContact.SiteLink;
                }
            }
            else
            {
                var newContact = new ShopContacts
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = request.ShopContact.PhoneNumber,
                    InstagramLink = request.ShopContact.InstagramLink,
                    Email = request.ShopContact.Email,
                    SiteLink = request.ShopContact.SiteLink
                };
                dbContext.ShopContacts.Add(newContact);
                shop.ShopContactId = newContact.Id;
            }
        }

        // Update or create Location
        if (request.Location != null)
        {
            if (shop.LocationId.HasValue)
            {
                var existingLocation = await dbContext.ModerationLocations.FindAsync([shop.LocationId.Value], cancellationToken);
                if (existingLocation != null)
                {
                    existingLocation.Address = request.Location.Address;
                    existingLocation.Latitude = request.Location.Latitude;
                    existingLocation.Longitude = request.Location.Longitude;
                }
            }
            else
            {
                var newLocation = new Location
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    Address = request.Location.Address,
                    Latitude = request.Location.Latitude,
                    Longitude = request.Location.Longitude
                };
                dbContext.ModerationLocations.Add(newLocation);
                shop.LocationId = newLocation.Id;
            }
        }

        // Update Schedules - remove old and create new
        if (request.Schedules != null)
        {
            var existingSchedules = await dbContext.ModerationShopSchedules
                .Include(s => s.Intervals)
                .Where(s => s.ShopId == shop.Id)
                .ToListAsync(cancellationToken);

            dbContext.ModerationShopSchedules.RemoveRange(existingSchedules);

            foreach (var scheduleDto in request.Schedules)
            {
                if (scheduleDto.DayOfWeek == null) continue;

                var schedule = new ModerationShopSchedule
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    DayOfWeek = scheduleDto.DayOfWeek.Value,
                    IsClosed = scheduleDto.Intervals == null || !scheduleDto.Intervals.Any()
                };

                if (!schedule.IsClosed && scheduleDto.Intervals != null)
                {
                    schedule.Intervals = scheduleDto.Intervals.Select(interval => new ModerationShopScheduleInterval
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = schedule.Id,
                        OpenTime = interval.OpenTime,
                        CloseTime = interval.CloseTime
                    }).ToList();
                }

                dbContext.ModerationShopSchedules.Add(schedule);
            }
        }

        // Update Equipment links - remove old and create new
        if (request.EquipmentIds != null)
        {
            var existingEquipments = await dbContext.ModerationShopEquipments
                .Where(e => e.ShopId == shop.Id)
                .ToListAsync(cancellationToken);
            dbContext.ModerationShopEquipments.RemoveRange(existingEquipments);

            foreach (var equipmentId in request.EquipmentIds)
            {
                dbContext.ModerationShopEquipments.Add(new ModerationShopEquipment
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    EquipmentId = equipmentId
                });
            }
        }

        // Update CoffeeBean links
        if (request.CoffeeBeanIds != null)
        {
            var existingBeans = await dbContext.ModerationCoffeeBeanShops
                .Where(cb => cb.ShopId == shop.Id)
                .ToListAsync(cancellationToken);
            dbContext.ModerationCoffeeBeanShops.RemoveRange(existingBeans);

            foreach (var coffeeBeanId in request.CoffeeBeanIds)
            {
                dbContext.ModerationCoffeeBeanShops.Add(new ModerationCoffeeBeanShop
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    CoffeeBeanId = coffeeBeanId
                });
            }
        }

        // Update Roaster links
        if (request.RoasterIds != null)
        {
            var existingRoasters = await dbContext.ModerationRoasterShops
                .Where(rs => rs.ShopId == shop.Id)
                .ToListAsync(cancellationToken);
            dbContext.ModerationRoasterShops.RemoveRange(existingRoasters);

            foreach (var roasterId in request.RoasterIds)
            {
                dbContext.ModerationRoasterShops.Add(new ModerationRoasterShop
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    RoasterId = roasterId
                });
            }
        }

        // Update BrewMethod links
        if (request.BrewMethodIds != null)
        {
            var existingBrewMethods = await dbContext.ModerationShopBrewMethods
                .Where(bm => bm.ShopId == shop.Id)
                .ToListAsync(cancellationToken);
            dbContext.ModerationShopBrewMethods.RemoveRange(existingBrewMethods);

            foreach (var brewMethodId in request.BrewMethodIds)
            {
                dbContext.ModerationShopBrewMethods.Add(new ModerationShopBrewMethod
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
                    BrewMethodId = brewMethodId
                });
            }
        }

        // Update ShopPhotos if provided (TODO: implement file upload logic)
        if (request.ShopPhotos != null && request.ShopPhotos.Any())
        {
            // Note: This is a placeholder - actual file upload logic should be implemented
        }

        await repository.UpdateAsync(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response<UpdateModerationCoffeeShopResponse>.Success(null);
    }
}