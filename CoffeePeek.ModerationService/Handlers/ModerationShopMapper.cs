using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.ModerationService.Models;

namespace CoffeePeek.ModerationService.Handlers;

public static class ModerationShopMapper
{
    public static ModerationShopDto MapToDto(ModerationShop shop)
    {
        return new ModerationShopDto
        {
            Id = shop.Id,
            Name = shop.Name,
            NotValidatedAddress = shop.NotValidatedAddress,
            UserId = shop.UserId,
            AddressId = shop.AddressId,
            ShopContactId = shop.ShopContactId,
            ModerationStatus = shop.ModerationStatus,
            Status = shop.Status,
            Address = shop.Address != null ? new AddressDto
            {
                CityId = shop.Address.CityId,
                StreetId = shop.Address.StreetId,
                BuildingNumber = shop.Address.BuildingNumber,
                PostalCode = shop.Address.PostalCode,
                Latitude = shop.Address.Latitude,
                Longitude = shop.Address.Longitude
            } : null,
            ShopContact = shop.ShopContacts != null ? new ShopContactDto
            {
                PhoneNumber = shop.ShopContacts.PhoneNumber,
                InstagramLink = shop.ShopContacts.InstagramLink
            } : null,
            ShopPhotos = shop.ShopPhotos.Select(p => p.Url).ToList(),
            Schedules = shop.Schedules.Select(s => new ScheduleDto
            {
                DayOfWeek = s.DayOfWeek,
                OpeningTime = s.OpeningTime,
                ClosingTime = s.ClosingTime
            }).ToList()
        };
    }
}

