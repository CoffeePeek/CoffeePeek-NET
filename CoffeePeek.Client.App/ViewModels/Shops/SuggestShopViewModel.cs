using System.Collections.ObjectModel;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Client.App.ViewModels.Shops.Search;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public partial class SuggestShopViewModel(
    IWebCatalogsClient catalogsClient,
    IWebModerationShopsClient moderationShopsClient,
    IImagePickerService imagePickerService,
    IWorkspaceShellNavigator shellNavigator) : ViewModelBase
{
    private readonly CatalogFilterGroupViewModel _beanFilters = new(Lang.SuggestShop_SectionCoffeeBeans);
    private readonly CatalogFilterGroupViewModel _roasterFilters = new(Lang.SuggestShop_SectionRoasters);
    private readonly CatalogFilterGroupViewModel _brewMethodFilters = new(Lang.SuggestShop_SectionBrewMethods);
    private readonly CatalogFilterGroupViewModel _equipmentFilters = new(Lang.SuggestShop_SectionEquipment);

    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial string Address { get; set; } = string.Empty;
    [ObservableProperty] public partial CityDto? SelectedCity { get; set; }
    [ObservableProperty] public partial bool IsLoadingCatalogs { get; set; }
    [ObservableProperty] public partial bool IsSubmitting { get; set; }
    [ObservableProperty] public partial string? ErrorMessage { get; set; }
    [ObservableProperty] public partial string? SuccessMessage { get; set; }

    [ObservableProperty] public partial bool IsAboutSectionOpen { get; set; }
    [ObservableProperty] public partial bool IsContactsSectionOpen { get; set; }
    [ObservableProperty] public partial bool IsScheduleSectionOpen { get; set; }
    [ObservableProperty] public partial bool IsCoffeeProfileSectionOpen { get; set; }
    [ObservableProperty] public partial bool IsPhotosSectionOpen { get; set; }

    [ObservableProperty] public partial string Description { get; set; } = string.Empty;
    [ObservableProperty] public partial PriceRange? SelectedPriceRange { get; set; }
    [ObservableProperty] public partial string ContactPhone { get; set; } = string.Empty;
    [ObservableProperty] public partial string ContactEmail { get; set; } = string.Empty;
    [ObservableProperty] public partial string ContactSite { get; set; } = string.Empty;
    [ObservableProperty] public partial string ContactInstagram { get; set; } = string.Empty;

    public ObservableCollection<CityDto> Cities { get; } = [];
    public ObservableCollection<PriceRange> PriceRanges { get; } = [.. Enum.GetValues<PriceRange>()];
    public ObservableCollection<CatalogFilterGroupViewModel> FilterGroups { get; } = [];
    public ObservableCollection<PickedImageFileViewModel> Photos { get; } = [];
    public ObservableCollection<DayScheduleInputViewModel> Schedules { get; } =
    [
        new(DayOfWeek.Monday),
        new(DayOfWeek.Tuesday),
        new(DayOfWeek.Wednesday),
        new(DayOfWeek.Thursday),
        new(DayOfWeek.Friday),
        new(DayOfWeek.Saturday),
        new(DayOfWeek.Sunday)
    ];

    public bool CanSubmit =>
        !IsSubmitting
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Address)
        && SelectedCity is not null;

    partial void OnNameChanged(string value) => OnPropertyChanged(nameof(CanSubmit));
    partial void OnAddressChanged(string value) => OnPropertyChanged(nameof(CanSubmit));
    partial void OnSelectedCityChanged(CityDto? value) => OnPropertyChanged(nameof(CanSubmit));
    partial void OnIsSubmittingChanged(bool value) => OnPropertyChanged(nameof(CanSubmit));

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (Cities.Count > 0)
            return;

        IsLoadingCatalogs = true;
        try
        {
            var citiesTask = catalogsClient.GetCitiesAsync(ct);
            var beansTask = catalogsClient.GetBeansAsync(ct);
            var roastersTask = catalogsClient.GetRoastersAsync(ct);
            var equipmentTask = catalogsClient.GetEquipmentAsync(ct);
            var brewMethodsTask = catalogsClient.GetBrewMethodsAsync(ct);

            await Task.WhenAll(citiesTask, beansTask, roastersTask, equipmentTask, brewMethodsTask);

            if (citiesTask.Result.IsSuccess)
            {
                foreach (var city in citiesTask.Result.Value.Cities)
                    Cities.Add(city);
                SelectedCity = Cities.FirstOrDefault();
            }

            if (beansTask.Result.IsSuccess)
                _beanFilters.ReplaceItems(beansTask.Result.Value.Beans.Select(b => new CatalogFilterItemViewModel { Id = b.Id, Name = b.Name }));
            if (roastersTask.Result.IsSuccess)
                _roasterFilters.ReplaceItems(roastersTask.Result.Value.Roasters.Select(r => new CatalogFilterItemViewModel { Id = r.Id, Name = r.Name }));
            if (equipmentTask.Result.IsSuccess)
                _equipmentFilters.ReplaceItems(equipmentTask.Result.Value.Equipment.Select(e => new CatalogFilterItemViewModel { Id = e.Id, Name = e.Name }));
            if (brewMethodsTask.Result.IsSuccess)
                _brewMethodFilters.ReplaceItems(brewMethodsTask.Result.Value.BrewMethods.Select(m => new CatalogFilterItemViewModel { Id = m.Id, Name = m.Name }));

            FilterGroups.Clear();
            foreach (var group in new[] { _beanFilters, _roasterFilters, _brewMethodFilters, _equipmentFilters })
                FilterGroups.Add(group);
        }
        finally
        {
            IsLoadingCatalogs = false;
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        var picked = await imagePickerService.PickImageAsync();
        if (picked is null)
            return;

        Photos.Add(new PickedImageFileViewModel(picked));
    }

    [RelayCommand]
    private void RemovePhoto(PickedImageFileViewModel photo)
    {
        Photos.Remove(photo);
    }

    [RelayCommand]
    private void ToggleSection(string sectionKey)
    {
        switch (sectionKey)
        {
            case "about":
                IsAboutSectionOpen = !IsAboutSectionOpen;
                break;
            case "contacts":
                IsContactsSectionOpen = !IsContactsSectionOpen;
                break;
            case "schedule":
                IsScheduleSectionOpen = !IsScheduleSectionOpen;
                break;
            case "coffee":
                IsCoffeeProfileSectionOpen = !IsCoffeeProfileSectionOpen;
                break;
            case "photos":
                IsPhotosSectionOpen = !IsPhotosSectionOpen;
                break;
        }
    }

    [RelayCommand]
    private async Task SubmitAsync(CancellationToken ct = default)
    {
        if (!CanSubmit)
        {
            ErrorMessage = Lang.SuggestShop_ValidationRequired;
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            IReadOnlyList<CoffeePeek.Contract.Dtos.UploadedPhotoDto>? uploadedPhotos = null;
            if (Photos.Count > 0)
            {
                var uploadResult = await moderationShopsClient.UploadShopPhotosAsync(
                    Photos.Select(p => new ShopPhotoBinaryPayload(
                        p.File.FileName,
                        p.File.ContentType,
                        p.File.Content))
                    .ToList(),
                    ct);

                if (uploadResult.IsFailed)
                {
                    ErrorMessage = uploadResult.Errors.FirstOrDefault()?.Message ?? Lang.SuggestShop_SubmitError;
                    return;
                }

                uploadedPhotos = uploadResult.Value;
            }

            var request = new SendCoffeeShopToModerationRequest
            {
                Name = Name.Trim(),
                Address = Address.Trim(),
                CityId = SelectedCity!.Id,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                PriceRange = SelectedPriceRange,
                ShopContact = BuildContact(),
                Schedules = BuildSchedules(),
                EquipmentIds = _equipmentFilters.SelectedIds?.ToList(),
                CoffeeBeanIds = _beanFilters.SelectedIds?.ToList(),
                RoasterIds = _roasterFilters.SelectedIds?.ToList(),
                BrewMethodIds = _brewMethodFilters.SelectedIds?.ToList(),
                ShopPhotos = uploadedPhotos?.ToList()
            };

            var sendResult = await moderationShopsClient.SendSuggestionAsync(request, ct);
            if (sendResult.IsFailed)
            {
                ErrorMessage = sendResult.Errors.FirstOrDefault()?.Message ?? Lang.SuggestShop_SubmitError;
                return;
            }

            SuccessMessage = Lang.SuggestShop_SubmitSuccess;
            ResetForm();
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void Close() => shellNavigator.CloseSuggestShop();

    private ShopContactDto? BuildContact()
    {
        var hasAny = !string.IsNullOrWhiteSpace(ContactPhone)
                     || !string.IsNullOrWhiteSpace(ContactEmail)
                     || !string.IsNullOrWhiteSpace(ContactSite)
                     || !string.IsNullOrWhiteSpace(ContactInstagram);
        if (!hasAny)
            return null;

        return new ShopContactDto
        {
            PhoneNumber = string.IsNullOrWhiteSpace(ContactPhone) ? null : ContactPhone.Trim(),
            Email = string.IsNullOrWhiteSpace(ContactEmail) ? null : ContactEmail.Trim(),
            SiteLink = string.IsNullOrWhiteSpace(ContactSite) ? null : ContactSite.Trim(),
            InstagramLink = string.IsNullOrWhiteSpace(ContactInstagram) ? null : ContactInstagram.Trim()
        };
    }

    private List<ScheduleDto>? BuildSchedules()
    {
        var result = new List<ScheduleDto>();
        foreach (var item in Schedules.Where(s => s.IsEnabled))
        {
            if (item.IsClosed)
            {
                result.Add(new ScheduleDto(item.DayOfWeek, true, null));
                continue;
            }

            if (!TimeSpan.TryParse(item.OpenTime, out var openTime) ||
                !TimeSpan.TryParse(item.CloseTime, out var closeTime))
                continue;

            result.Add(new ScheduleDto(
                item.DayOfWeek,
                false,
                [new ShopScheduleIntervalDto { OpenTime = openTime, CloseTime = closeTime }]));
        }

        return result.Count > 0 ? result : null;
    }

    private void ResetForm()
    {
        Name = string.Empty;
        Address = string.Empty;
        Description = string.Empty;
        SelectedPriceRange = null;
        ContactPhone = string.Empty;
        ContactEmail = string.Empty;
        ContactSite = string.Empty;
        ContactInstagram = string.Empty;
        Photos.Clear();
        foreach (var schedule in Schedules)
            schedule.Reset();
        foreach (var group in FilterGroups)
            group.ClearSelection();
    }
}

public sealed class PickedImageFileViewModel(PickedImageFile file) : ViewModelBase
{
    public PickedImageFile File { get; } = file;
    public string Name => File.FileName;
}

public sealed partial class DayScheduleInputViewModel(DayOfWeek dayOfWeek) : ViewModelBase
{
    public DayOfWeek DayOfWeek { get; } = dayOfWeek;
    public string DayLabel => dayOfWeek switch
    {
        DayOfWeek.Monday => Lang.SuggestShop_DayMonday,
        DayOfWeek.Tuesday => Lang.SuggestShop_DayTuesday,
        DayOfWeek.Wednesday => Lang.SuggestShop_DayWednesday,
        DayOfWeek.Thursday => Lang.SuggestShop_DayThursday,
        DayOfWeek.Friday => Lang.SuggestShop_DayFriday,
        DayOfWeek.Saturday => Lang.SuggestShop_DaySaturday,
        DayOfWeek.Sunday => Lang.SuggestShop_DaySunday,
        _ => dayOfWeek.ToString()
    };

    [ObservableProperty] public partial bool IsEnabled { get; set; }
    [ObservableProperty] public partial bool IsClosed { get; set; }
    [ObservableProperty] public partial string OpenTime { get; set; } = "08:00";
    [ObservableProperty] public partial string CloseTime { get; set; } = "22:00";

    public void Reset()
    {
        IsEnabled = false;
        IsClosed = false;
        OpenTime = "08:00";
        CloseTime = "22:00";
    }
}
