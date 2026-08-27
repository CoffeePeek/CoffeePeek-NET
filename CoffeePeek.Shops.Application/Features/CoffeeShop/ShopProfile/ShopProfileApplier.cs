using System.Net;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Admin.Shops;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;
using DomainCoffeeShop = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShop;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.ShopProfile;

public static class ShopProfileApplier
{
    public static async Task<Response<AdminPublishedShopDto>?> ApplyAsync(
        DomainCoffeeShop shop,
        ShopProfilePatch patch,
        IQueryCityRepository cities,
        IQueryEquipmentRepository equipment,
        IQueryCoffeeBeanRepository beans,
        IQueryRoasterRepository roasters,
        IQueryBrewMethodRepository brewMethods,
        CancellationToken ct)
    {
        if (patch.Location is not null)
        {
            if (!await cities.Exists(patch.Location.CityId, ct))
                return Response<AdminPublishedShopDto>.Error(HttpStatusCode.BadRequest, "City was not found.");

            try
            {
                shop.SetLocation(
                    patch.Location.CityId,
                    patch.Location.Address,
                    patch.Location.Latitude,
                    patch.Location.Longitude);
            }
            catch (DomainException ex)
            {
                return Response<AdminPublishedShopDto>.Error(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        if (patch.Contacts is not null)
        {
            try
            {
                shop.SetContact(
                    patch.Contacts.InstagramLink,
                    patch.Contacts.Email,
                    patch.Contacts.SiteLink,
                    patch.Contacts.PhoneNumber);
            }
            catch (DomainException ex)
            {
                return Response<AdminPublishedShopDto>.Error(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        if (patch.Schedules is not null)
        {
            var schedules = new List<ShopSchedule>();
            foreach (var day in patch.Schedules)
            {
                var intervals = (day.Intervals ?? [])
                    .Select(i => ShopScheduleInterval.Create(i.OpenTime, i.CloseTime))
                    .ToList();
                schedules.Add(ShopSchedule.Create(day.DayOfWeek, day.IsClosed, intervals));
            }

            var replaced = shop.ReplaceSchedules(schedules);
            if (replaced.IsFailed)
                return Response<AdminPublishedShopDto>.Error(HttpStatusCode.BadRequest, replaced.Errors[0].Message);
        }

        if (patch.Catalogs is not null)
        {
            var catalogError = await ApplyCatalogsAsync(
                shop, patch.Catalogs, equipment, beans, roasters, brewMethods, ct);
            if (catalogError is not null)
                return catalogError;
        }

        return null;
    }

    private static async Task<Response<AdminPublishedShopDto>?> ApplyCatalogsAsync(
        DomainCoffeeShop shop,
        ShopCatalogsPatch catalogs,
        IQueryEquipmentRepository equipment,
        IQueryCoffeeBeanRepository beans,
        IQueryRoasterRepository roasters,
        IQueryBrewMethodRepository brewMethods,
        CancellationToken ct)
    {
        if (catalogs.EquipmentIds is not null)
        {
            var loaded = await LoadDistinctAsync(
                catalogs.EquipmentIds,
                ids => equipment.GetByIds(ids, ct),
                "equipment");
            if (loaded.Error is not null)
                return loaded.Error;
            shop.SetEquipment(loaded.Items);
        }

        if (catalogs.BeanIds is not null)
        {
            var loaded = await LoadDistinctAsync(
                catalogs.BeanIds,
                ids => beans.GetByIds(ids, ct),
                "coffee beans");
            if (loaded.Error is not null)
                return loaded.Error;
            shop.SetBeans(loaded.Items);
        }

        if (catalogs.RoasterIds is not null)
        {
            var loaded = await LoadDistinctAsync(
                catalogs.RoasterIds,
                ids => roasters.GetByIds(ids, ct),
                "roasters");
            if (loaded.Error is not null)
                return loaded.Error;
            shop.SetRoasters(loaded.Items);
        }

        if (catalogs.BrewMethodIds is not null)
        {
            var loaded = await LoadDistinctAsync(
                catalogs.BrewMethodIds,
                ids => brewMethods.GetByIds(ids, ct),
                "brew methods");
            if (loaded.Error is not null)
                return loaded.Error;
            shop.SetBrewMethods(loaded.Items);
        }

        return null;
    }

    private static async Task<(IReadOnlyList<T> Items, Response<AdminPublishedShopDto>? Error)> LoadDistinctAsync<T>(
        IReadOnlyList<Guid> ids,
        Func<List<Guid>, Task<IEnumerable<T>>> load,
        string catalogName)
        where T : class
    {
        var distinct = ids.Distinct().ToList();
        if (distinct.Count == 0)
            return ([], null);

        var items = (await load(distinct)).ToList();
        if (items.Count != distinct.Count)
        {
            return ([], Response<AdminPublishedShopDto>.Error(
                HttpStatusCode.BadRequest,
                $"One or more {catalogName} IDs were not found."));
        }

        return (items, null);
    }
}
