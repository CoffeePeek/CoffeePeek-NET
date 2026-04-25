namespace CoffeePeek.Client.App.ViewModels.WelcomeFlow.Welcome;

public class WelcomePageCardItemViewModel
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    public WelcomePageCardItemViewModel()
    {
        
    }

    public WelcomePageCardItemViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }
}