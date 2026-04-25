using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CoffeePeek.Client.App.ViewModels.Abstract;

namespace CoffeePeek.Client.App.ViewModels.Shops.Search;

public sealed class CatalogFilterGroupViewModel : ViewModelBase
{
    public CatalogFilterGroupViewModel(string title)
    {
        Title = title;
        Items.CollectionChanged += OnItemsChanged;
    }

    public string Title { get; }

    public ObservableCollection<CatalogFilterItemViewModel> Items { get; } = [];

    public bool HasItems => Items.Count > 0;

    public int ActiveFilterCount => Items.Count(i => i.IsSelected);

    public Guid[]? SelectedIds => GetSelectedIds(Items);

    public void ReplaceItems(IEnumerable<CatalogFilterItemViewModel> items)
    {
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }

    public void ClearSelection()
    {
        foreach (var item in Items)
            item.IsSelected = false;
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CatalogFilterItemViewModel item in e.OldItems)
                item.PropertyChanged -= OnItemPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (CatalogFilterItemViewModel item in e.NewItems)
                item.PropertyChanged += OnItemPropertyChanged;
        }

        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(ActiveFilterCount));
        OnPropertyChanged(nameof(SelectedIds));
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CatalogFilterItemViewModel.IsSelected))
            return;

        OnPropertyChanged(nameof(ActiveFilterCount));
        OnPropertyChanged(nameof(SelectedIds));
    }

    internal static Guid[]? GetSelectedIds(IEnumerable<CatalogFilterItemViewModel> filters)
    {
        var selected = filters.Where(f => f.IsSelected).Select(f => f.Id).ToArray();
        return selected.Length > 0 ? selected : null;
    }
}
