using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using Coffeepeek.Moderation.Application.Commands;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.ModerationService.Services.Interfaces;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coffeepeek.Moderation.Application.Handlers;

public class UpdateModerationCoffeeShopHandler(
    IModerationShopRepository repository,
    IUnitOfWork unitOfWork,
    IGenericRepository<ShopContacts> shopContactsRepository,
    IGenericRepository<Location> locationsRepository,
    IGenericRepository<ModerationShopSchedule> moderationShopScheduleRepository,
    IGenericRepository<ModerationShopEquipment> moderationShopEquipmentRepository,
    IGenericRepository<ModerationCoffeeBeanShop> moderationCoffeeBeanShopRepository,
    IGenericRepository<ModerationRoasterShop> moderationRoasterShopRepository,
    IGenericRepository<ModerationShopBrewMethod> moderationShopBrewRepository,
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
                var existingContact = await shopContactsRepository
                    .FirstOrDefaultAsync(x => x.Id == shop.ShopContactId.Value, cancellationToken);
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
                shopContactsRepository.Add(newContact);
                shop.ShopContactId = newContact.Id;
            }
        }

        // Update or create Location
        if (request.Location != null)
        {
            if (shop.LocationId.HasValue)
            {
                var existingLocation = await locationsRepository
                    .FirstOrDefaultAsync(x => x.Id  == shop.LocationId.Value, cancellationToken);
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
                locationsRepository.Add(newLocation);
                shop.LocationId = newLocation.Id;
            }
        }

        // Update Schedules - remove old and create new
        if (request.Schedules != null)
        {
            var existingSchedules = await moderationShopScheduleRepository
                .Query()
                .Include(s => s.Intervals)
                .Where(s => s.ShopId == shop.Id)
                .ToListAsync(cancellationToken);

            moderationShopScheduleRepository.RemoveRange(existingSchedules);

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

                moderationShopScheduleRepository.Add(schedule);
            }
        }

        // Update Equipment links - remove old and create new
        if (request.EquipmentIds != null)
        {
            var existingEquipments = await moderationShopEquipmentRepository
                .FindAsNoTrackingAsync(x => x.ShopId == shop.Id, cancellationToken);
            moderationShopEquipmentRepository.RemoveRange(existingEquipments);

            foreach (var equipmentId in request.EquipmentIds)
            {
                moderationShopEquipmentRepository.Add(new ModerationShopEquipment
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
            var existingBeans = await moderationCoffeeBeanShopRepository
                .FindAsync(x => x.ShopId == shop.Id, cancellationToken);
            moderationCoffeeBeanShopRepository.RemoveRange(existingBeans);

            foreach (var coffeeBeanId in request.CoffeeBeanIds)
            {
                moderationCoffeeBeanShopRepository.Add(new ModerationCoffeeBeanShop
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
            var existingRoasters = await moderationRoasterShopRepository
                .FindAsync(rs => rs.ShopId == shop.Id, cancellationToken);
            moderationRoasterShopRepository.RemoveRange(existingRoasters);

            foreach (var roasterId in request.RoasterIds)
            {
                moderationRoasterShopRepository.Add(new ModerationRoasterShop
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
            var existingBrewMethods = await moderationShopBrewRepository
                .FindAsync(bm => bm.ShopId == shop.Id);
            moderationShopBrewRepository.RemoveRange(existingBrewMethods);

            foreach (var brewMethodId in request.BrewMethodIds)
            {
                moderationShopBrewRepository.Add(new ModerationShopBrewMethod
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