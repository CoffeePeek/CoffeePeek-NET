using System.Collections.ObjectModel;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Client.App.Services;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public partial class ModerationPanelViewModel(
    IWebModerationPanelClient moderationPanelClient,
    IWorkspaceShellNavigator shellNavigator) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool HasEmptyShops { get; private set; }

    [ObservableProperty]
    public partial bool HasEmptyReviews { get; private set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<ModerationShopRowViewModel> Shops { get; } = [];
    public ObservableCollection<ModerationReviewRowViewModel> Reviews { get; } = [];

    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var shopsTask = moderationPanelClient.GetAllShopsAsync(ct);
            var reviewsTask = moderationPanelClient.GetAllReviewsAsync(ct);
            await Task.WhenAll(shopsTask, reviewsTask);
            var shopsResult = await shopsTask;
            var reviewsResult = await reviewsTask;

            if (shopsResult.IsFailed)
            {
                ErrorMessage = shopsResult.Errors.FirstOrDefault()?.Message ?? Lang.Moderation_LoadError;
                return;
            }

            if (reviewsResult.IsFailed)
            {
                ErrorMessage = reviewsResult.Errors.FirstOrDefault()?.Message ?? Lang.Moderation_LoadError;
                return;
            }

            Shops.Clear();
            foreach (var s in shopsResult.Value)
                Shops.Add(new ModerationShopRowViewModel(s, ApproveShopCommand, RejectShopCommand));

            Reviews.Clear();
            foreach (var r in reviewsResult.Value)
                Reviews.Add(new ModerationReviewRowViewModel(r, ApproveReviewCommand, RejectReviewCommand));

            HasEmptyShops = Shops.Count == 0;
            HasEmptyReviews = Reviews.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken) => await LoadAsync(cancellationToken);

    [RelayCommand]
    private void Close() => shellNavigator.CloseModerationPanel();

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task ApproveShopAsync(ModerationShopRowViewModel? row, CancellationToken cancellationToken)
    {
        if (row is null || !row.CanModerate)
            return;

        ErrorMessage = null;
        var result = await moderationPanelClient.UpdateShopStatusAsync(
            row.Model.Id,
            ModerationStatus.Approved,
            cancellationToken);
        if (result.IsFailed)
            ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.Moderation_ActionFailed;
        else
            await LoadAsync(cancellationToken);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task RejectShopAsync(ModerationShopRowViewModel? row, CancellationToken cancellationToken)
    {
        if (row is null || !row.CanModerate)
            return;

        ErrorMessage = null;
        var result = await moderationPanelClient.UpdateShopStatusAsync(
            row.Model.Id,
            ModerationStatus.Rejected,
            cancellationToken);
        if (result.IsFailed)
            ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.Moderation_ActionFailed;
        else
            await LoadAsync(cancellationToken);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task ApproveReviewAsync(ModerationReviewRowViewModel? row, CancellationToken cancellationToken)
    {
        if (row is null || !row.CanModerate)
            return;

        ErrorMessage = null;
        var result = await moderationPanelClient.ChangeReviewStatusAsync(
            row.Model.Id,
            ModerationStatus.Approved,
            null,
            cancellationToken);
        if (result.IsFailed)
            ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.Moderation_ActionFailed;
        else
            await LoadAsync(cancellationToken);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task RejectReviewAsync(ModerationReviewRowViewModel? row, CancellationToken cancellationToken)
    {
        if (row is null || !row.CanModerate)
            return;

        ErrorMessage = null;
        var reason = string.IsNullOrWhiteSpace(row.RejectReason) ? Lang.Moderation_DefaultRejectReason : row.RejectReason.Trim();
        var result = await moderationPanelClient.ChangeReviewStatusAsync(
            row.Model.Id,
            ModerationStatus.Rejected,
            reason,
            cancellationToken);
        if (result.IsFailed)
            ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? Lang.Moderation_ActionFailed;
        else
            await LoadAsync(cancellationToken);
    }
}
