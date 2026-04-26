using System.Windows.Input;
using CoffeePeek.Client.App.ViewModels.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using Lang = CoffeePeek.Client.App.Resources.Lang.Resources;

namespace CoffeePeek.Client.App.ViewModels.Home;

public sealed class ModerationShopRowViewModel : ViewModelBase
{
    public ModerationShopRowViewModel(
        ModerationShopDto dto,
        ICommand approveCommand,
        ICommand rejectCommand)
    {
        Model = dto;
        ApproveCommand = approveCommand;
        RejectCommand = rejectCommand;
    }

    public ModerationShopDto Model { get; }

    public ICommand ApproveCommand { get; }

    public ICommand RejectCommand { get; }

    public string Name => Model.Name;

    public string? Address => Model.Address;

    public string StatusLabel => Model.ModerationStatus switch
    {
        ModerationStatus.Pending => Lang.ModerationStatus_Pending,
        ModerationStatus.Approved => Lang.ModerationStatus_Approved,
        ModerationStatus.Rejected => Lang.ModerationStatus_Rejected,
        _ => Model.ModerationStatus.ToString()
    };

    public bool CanModerate => Model.ModerationStatus == ModerationStatus.Pending;
}
