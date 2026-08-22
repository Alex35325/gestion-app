using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GestionApp.Data;
using GestionApp.Models;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

/// <summary>
/// Shared list+search+CRUD behavior for Revenus and Dépenses. Each is shown
/// three ways on the website (scoped to a client, a véhicule, or a produit)
/// — six screens total that only differ in which table, which filter
/// predicate, and which dialog to open. This base class holds everything
/// that's identical between them; RevenusViewModel/DepensesViewModel supply
/// just those three differences. Extending the app with a new scope (say,
/// a "Projets" entity later) means adding one filter predicate, not
/// duplicating list/search/CRUD logic again.
/// </summary>
public abstract class TransactionsViewModelBase<T> : ViewModelBase where T : class, ITransaction
{
    protected readonly AppDataStore Store;
    private readonly Func<T, bool> _scopeFilter;
    private readonly Action<string> _setStatus;

    protected abstract string Table { get; }
    protected abstract ObservableCollection<T> Source { get; }
    protected abstract T? ShowDialog(T? editing);

    public string Title { get; }
    public string SearchPlaceholder { get; }
    public ICollectionView Items { get; }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { if (SetField(ref _searchText, value)) Items.Refresh(); }
    }

    private T? _selectedItem;
    public T? SelectedItem
    {
        get => _selectedItem;
        set { if (SetField(ref _selectedItem, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    private string _totalText = "";
    public string TotalText { get => _totalText; private set => SetField(ref _totalText, value); }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    protected TransactionsViewModelBase(AppDataStore store, Func<T, bool> scopeFilter, string title, string searchPlaceholder, Action<string> setStatus)
    {
        Store = store;
        _scopeFilter = scopeFilter;
        _setStatus = setStatus;
        Title = title;
        SearchPlaceholder = searchPlaceholder;

        Items = CollectionViewSource.GetDefaultView(Source);
        Items.Filter = o => o is T t && _scopeFilter(t) && Matches(t);

        AddCommand = new AsyncRelayCommand(AddAsync);
        EditCommand = new AsyncRelayCommand(EditAsync, () => SelectedItem != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem != null);

        RecomputeTotal();
    }

    private bool Matches(T t)
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var q = SearchText.Trim();
        return Has(t.ClientName, q) || Has(t.VehiculeName, q) || Has(t.ProduitName, q) || Has(t.Categorie, q) || Has(t.Description, q);
    }

    private static bool Has(string? s, string q) => !string.IsNullOrEmpty(s) && s.Contains(q, StringComparison.OrdinalIgnoreCase);

    private void RecomputeTotal()
        => TotalText = "Total : " + Source.Where(_scopeFilter).Sum(t => t.Montant).ToString("C", new CultureInfo("fr-CA"));

    private void RelinkNames(T t)
    {
        t.ClientName = Store.ClientName(t.ClientId);
        t.VehiculeName = Store.VehiculeName(t.VehiculeId);
        t.ProduitName = Store.ProduitName(t.ProduitId);
    }

    private async Task AddAsync()
    {
        var result = ShowDialog(null);
        if (result == null) return;
        RelinkNames(result);
        var ok = await Store.InsertAsync(Source, Table, result);
        Items.Refresh();
        RecomputeTotal();
        _setStatus(ok ? "Ajouté." : "Échec de l'ajout : " + Store.LastError);
    }

    private async Task EditAsync()
    {
        if (SelectedItem == null) return;
        var previous = SelectedItem;
        var updated = ShowDialog(previous);
        if (updated == null) return;
        RelinkNames(updated);
        var ok = await Store.UpdateAsync(Source, Table, previous, updated);
        Items.Refresh();
        RecomputeTotal();
        _setStatus(ok ? "Modifié." : "Échec de la modification : " + Store.LastError);
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        if (MessageBox.Show(Application.Current.MainWindow, "Supprimer cet élément ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var item = SelectedItem;
        var ok = await Store.DeleteAsync(Source, Table, item);
        Items.Refresh();
        RecomputeTotal();
        _setStatus(ok ? "Supprimé." : "Échec de la suppression : " + Store.LastError);
    }
}
