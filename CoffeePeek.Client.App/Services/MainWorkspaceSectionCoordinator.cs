using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeePeek.Client.App.Services;

/// <summary>
/// Shared tab index for header and workspace (catalog / favorites / near me).
/// </summary>
public sealed partial class MainWorkspaceSectionCoordinator : ObservableObject
{
    [NotifyPropertyChangedFor(nameof(Section))]
    [ObservableProperty]
    public partial int SelectedIndex { get; set; }

    public MainWorkspaceSection Section =>
        Enum.IsDefined(typeof(MainWorkspaceSection), SelectedIndex)
            ? (MainWorkspaceSection)SelectedIndex
            : MainWorkspaceSection.Catalog;
}
