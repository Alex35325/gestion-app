using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GestionApp.Data;
using GestionApp.Models;
using GestionApp.Mvvm;
using GestionApp.Services;

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

    private bool _isNavPopupOpen;
    public bool IsNavPopupOpen { get => _isNavPopupOpen; set => SetField(ref _isNavPopupOpen, value); }

    public ICommand SelectNavCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleNavPopupCommand { get; }
    public ICommand MoveNavUpCommand { get; }
    public ICommand MoveNavDownCommand { get; }

    public MainViewModel()
    {
        BuildNav();
        ApplyNavPreferences();

        SelectNavCommand = new RelayCommand(p => { if (p is NavNode n) Select(n); });
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        ToggleNavPopupCommand = new RelayCommand(() => IsNavPopupOpen = !IsNavPopupOpen);
        MoveNavUpCommand = new RelayCommand(p => { if (p is NavNode n) MoveTopLevel(n, -1); });
        MoveNavDownCommand = new RelayCommand(p => { if (p is NavNode n) MoveTopLevel(n, +1); });

        _ = LoadAsync();
    }

    private void SetStatus(string text) => StatusText = text;

    private void BuildNav()
    {
        var res = Application.Current.Resources;
        Brush Dot(string key) => (Brush)res[key];

        var dashboard = new NavNode("dashboard", "Tableau de bord", () => new DashboardViewModel(Store)) { AccentBrush = Dot("NavDotDashboard") };

        var clients = new NavNode("clients", "Clients", () => new ClientsViewModel(Store, SetStatus)) { AccentBrush = Dot("NavDotClients") };
        clients.Children.Add(new NavNode("clients-revenus", "Revenus", () => new RevenusViewModel(Store, r => r.ClientId != null, "Revenus — Clients", SetStatus)));
        clients.Children.Add(new NavNode("clients-depenses", "Dépenses", () => new DepensesViewModel(Store, d => d.ClientId != null, "Dépenses — Clients", SetStatus)));

        var vehicules = new NavNode("vehicules", "Véhicules", () => new VehiculesInfoViewModel(Store, SetStatus)) { AccentBrush = Dot("NavDotVehicules") };
        vehicules.Children.Add(new NavNode("vehicules-revenus", "Revenus", () => new RevenusViewModel(Store, r => r.VehiculeId != null, "Revenus — Véhicules", SetStatus)));
        vehicules.Children.Add(new NavNode("vehicules-depenses", "Dépenses", () => new DepensesViewModel(Store, d => d.VehiculeId != null, "Dépenses — Véhicules", SetStatus)));
        vehicules.Children.Add(new NavNode("vehicules-maintenance", "Maintenance", () => new MaintenanceViewModel(Store, SetStatus)));

        var inventaire = new NavNode("inventaire", "Inventaire", () => new InventaireViewModel(Store, SetStatus)) { AccentBrush = Dot("NavDotInventaire") };
        inventaire.Children.Add(new NavNode("inventaire-revenus", "Revenus", () => new RevenusViewModel(Store, r => r.ProduitId != null, "Revenus — Inventaire", SetStatus)));
        inventaire.Children.Add(new NavNode("inventaire-depenses", "Dépenses", () => new DepensesViewModel(Store, d => d.ProduitId != null, "Dépenses — Inventaire", SetStatus)));

        var rentabilite = new NavNode("rentabilite", "Rentabilité", () => new RentabiliteViewModel(Store)) { AccentBrush = Dot("NavDotRentabilite") };

        NavItems.Add(dashboard);
        NavItems.Add(clients);
        NavItems.Add(vehicules);
        NavItems.Add(inventaire);
        NavItems.Add(rentabilite);

        foreach (var node in AllNodes(NavItems))
            node.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(NavNode.IsVisible)) SaveNavPreferences(); };

        // Set once here, before the TreeView (and its TwoWay IsSelected binding)
        // exists — safe. Select() below never touches IsSelected itself; see its
        // comment for why.
        dashboard.IsSelected = true;
        CurrentViewModel = dashboard.CreateViewModel!();
    }

    /// <summary>Applies the saved per-device order (top-level only) and
    /// visibility (all levels) — call once, right after BuildNav().</summary>
    private void ApplyNavPreferences()
    {
        var prefs = PreferencesService.Load();
        if (prefs.NavItems.Count == 0) return;

        foreach (var node in AllNodes(NavItems))
        {
            var saved = prefs.NavItems.FirstOrDefault(p => p.Id == node.Id);
            if (saved != null) node.IsVisible = saved.Visible;
        }

        // Reorder top-level items to match the saved sequence; anything not
        // mentioned (e.g. a section added in a later version) keeps its
        // default position at the end.
        var ordered = prefs.NavItems
            .Select(p => NavItems.FirstOrDefault(n => n.Id == p.Id))
            .Where(n => n != null)
            .Cast<NavNode>()
            .ToList();
        foreach (var extra in NavItems.Where(n => !ordered.Contains(n))) ordered.Add(extra);
        for (var i = 0; i < ordered.Count; i++)
        {
            var currentIndex = NavItems.IndexOf(ordered[i]);
            if (currentIndex != i) NavItems.Move(currentIndex, i);
        }
    }

    private void SaveNavPreferences()
    {
        var prefs = PreferencesService.Load();
        prefs.NavItems = AllNodes(NavItems).Select(n => new NavItemPref { Id = n.Id, Visible = n.IsVisible }).ToList();
        // Preserve top-level display order as the list order.
        var topLevelOrder = NavItems.Select(n => n.Id).ToList();
        prefs.NavItems = prefs.NavItems
            .OrderBy(p => topLevelOrder.Contains(p.Id) ? topLevelOrder.IndexOf(p.Id) : int.MaxValue)
            .ToList();
        PreferencesService.Save(prefs);
    }

    private void MoveTopLevel(NavNode node, int delta)
    {
        var index = NavItems.IndexOf(node);
        var newIndex = index + delta;
        if (index < 0 || newIndex < 0 || newIndex >= NavItems.Count) return;
        NavItems.Move(index, newIndex);
        SaveNavPreferences();
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
