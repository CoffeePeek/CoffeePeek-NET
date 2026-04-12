using System.Collections.ObjectModel;

namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

public class WelcomePageViewModel : ViewModelBase
{
    public ObservableCollection<WelcomePageCardItemViewModel> Features { get; } = [];

    public WelcomePageViewModel()
    {
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card1Title, Resources.Lang.Resources.WelcomePage_Card1Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card2Title, Resources.Lang.Resources.WelcomePage_Card2Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card3Title, Resources.Lang.Resources.WelcomePage_Card3Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card4Title, Resources.Lang.Resources.WelcomePage_Card4Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card5Title, Resources.Lang.Resources.WelcomePage_Card5Description));
        Features.Add(new WelcomePageCardItemViewModel(Resources.Lang.Resources.WelcomePage_Card6Title, Resources.Lang.Resources.WelcomePage_Card6Description));
    }
}