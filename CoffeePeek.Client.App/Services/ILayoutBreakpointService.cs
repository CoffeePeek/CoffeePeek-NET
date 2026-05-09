using System.ComponentModel;
using Avalonia;

namespace CoffeePeek.Client.App.Services;

public interface ILayoutBreakpointService : INotifyPropertyChanged
{
    bool IsCompact { get; }

    Thickness WorkspaceChromeMargin { get; }

    void UpdateViewportWidth(double width);
}
