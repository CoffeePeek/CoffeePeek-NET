using System.Windows.Input;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public sealed partial class ModerationReviewRowViewModel : ViewModelBase
{
    public ModerationReviewRowViewModel(
        ModerationReviewDto dto,
        ICommand approveCommand,
        ICommand rejectCommand)
    {
        Model = dto;
        ApproveCommand = approveCommand;
        RejectCommand = rejectCommand;
    }

    public ModerationReviewDto Model { get; }

    public ICommand ApproveCommand { get; }

    public ICommand RejectCommand { get; }

    public string Header => Model.Header;

    public string UserName => Model.UserName;

    public string StatusLabel => Model.ModerationStatus switch
    {
        ModerationStatus.Pending => Lang.ModerationStatus_Pending,
        ModerationStatus.Approved => Lang.ModerationStatus_Approved,
        ModerationStatus.Rejected => Lang.ModerationStatus_Rejected,
        _ => Model.ModerationStatus.ToString()
    };

    public bool CanModerate => Model.ModerationStatus == ModerationStatus.Pending;

    [ObservableProperty]
    public partial string RejectReason { get; set; } = string.Empty;
}
