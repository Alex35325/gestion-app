using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GestionApp.Data;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

/// <summary>
/// Root view model: owns the single AppDataStore, builds the nav tree once,
/// and swaps CurrentViewModel as the user navigates. Mirrors the website's
/// NAV_ITEMS + activeView + viewFns map, but as typed objects instead of
/// string ids and an innerHTML rebuild.
/// </summary>
public class MainViewModel : ViewModelBase
{
    public AppDataStore Store { get; } = new();
    public ObservableCollection<NavNode> NavItems { get; } = new();

    private object? _currentViewModel;
    public object? CurrentViewModel { get => _currentViewModel; private set => SetField(ref _currentViewModel, value); }

    private string _statusText = "Chargement...";
    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }

    public ICommand SelectNavCommand { get; }
    public ICommand RefreshCommand { get; }

    public MainViewModel()
    {
        BuildNav();
        SelectNavCommand = new RelayCommand(p => { if (p is NavNode n) Select(n); });
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        _ = LoadAsync();
    }

    private void SetStatus(string text) => StatusText = text;

    private void BuildNav()
    {
        var dashboard = new NavNode("Tableau de bord", () => new DashboardViewModel(Store));

        var clients = new NavNode("Clients", () => new ClientsViewModel(Store, SetStatus));
        clients.Children.Add(new NavNode("Revenus", () => new RevenusViewModel(Store, r => r.ClientId != null, "Revenus — Clients", SetStatus)));
        clients.Children.Add(new NavNode("Dépenses", () => new DepensesViewModel(Store, d => d.ClientId != null, "Dépenses — Clients", SetStatus)));

        var vehicules = new NavNode("Véhicules", () => new VehiculesInfoViewModel(Store, SetStatus));
        vehicules.Children.Add(new NavNode("Revenus", () => new RevenusViewModel(Store, r => r.VehiculeId != null, "Revenus — Véhicules", SetStatus)));
        vehicules.Children.Add(new NavNode("Dépenses", () => new DepensesViewModel(Store, d => d.VehiculeId != null, "Dépenses — Véhicules", SetStatus)));
        vehicules.Children.Add(new NavNode("Maintenance", () => new MaintenanceViewModel(Store, SetStatus)));

        var inventaire = new NavNode("Inventaire", () => new InventaireViewModel(Store, SetStatus));
        inventaire.Children.Add(new NavNode("Revenus", () => new RevenusViewModel(Store, r => r.ProduitId != null, "Revenus — Inventaire", SetStatus)));
        inventaire.Children.Add(new NavNode("Dépenses", () => new DepensesViewModel(Store, d => d.ProduitId != null, "Dépenses — Inventaire", SetStatus)));

        var rentabilite = new NavNode("Rentabilité", () => new RentabiliteViewModel(Store));

        NavItems.Add(dashboard);
        NavItems.Add(clients);
        NavItems.Add(vehicules);
        NavItems.Add(inventaire);
        NavItems.Add(rentabilite);

        // Set once here, before the TreeView (and its TwoWay IsSelected binding)
        // exists — safe. Select() below never touches IsSelected itself; see its
        // comment for why.
        dashboard.IsSelected = true;
        CurrentViewModel = dashboard.CreateViewModel!();
    }

    /// <summary>
    /// Called only in reaction to TreeView's own SelectedItemChanged — by the time
    /// this runs, WPF has already set NavNode.IsSelected via the TwoWay style
    /// binding and enforced single-selection across siblings itself. This method
    /// must NOT write back to IsSelected: doing so re-enters WPF's selection
    /// machinery while it's still on the call stack and stack-overflows.
    /// </summary>
    private void Select(NavNode node)
    {
        if (node.Children.Count > 0) node.IsExpanded = true;
        if (node.CreateViewModel != null) CurrentViewModel = node.CreateViewModel();
    }

    private static IEnumerable<NavNode> AllNodes(IEnumerable<NavNode> nodes)
    {
        foreach (var n in nodes)
        {
            yield return n;
            foreach (var c in AllNodes(n.Children)) yield return c;
        }
    }

    private async Task LoadAsync()
    {
        StatusText = "Chargement...";
        await Store.LoadAllAsync();
        StatusText = Store.LastError == null
            ? $"Prêt — {Store.Clients.Count} clients, {Store.Vehicules.Count} véhicules, {Store.Inventaire.Count} articles"
            : "Erreur : " + Store.LastError;

        // Re-render whatever tab is currently open against the freshly loaded data.
        var selected = AllNodes(NavItems).FirstOrDefault(n => n.IsSelected);
        if (selected?.CreateViewModel != null) CurrentViewModel = selected.CreateViewModel();
    }
}
