using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GestionApp.Data;
using GestionApp.Models;
using GestionApp.Mvvm;
using GestionApp.Services;

namespace GestionApp.ViewModels;

public class ActivityRow
{
    public string Date { get; set; } = "";
    public string Type { get; set; } = "";
    public string Categorie { get; set; } = "";
    public decimal Montant { get; set; }
    public string Description { get; set; } = "";
}

/// <summary>
/// Dashboard KPIs are "widgets" the user can show/hide right on the page —
/// unlike the website's first version, this customization panel lives on
/// the dashboard itself (IsCustomizing toggles it), not in a separate
/// Paramètres screen, so toggling a checkbox is visible immediately.
/// </summary>
public class DashboardViewModel : ViewModelBase
{
    private readonly AppDataStore _store;
    private static readonly CultureInfo Fr = new("fr-CA");
    private bool _loaded;

    public string CompanyName => _store.Settings.CompanyName;

    public ObservableCollection<DashboardWidget> Widgets { get; } = new();
    public ObservableCollection<InventaireItem> LowStockItems { get; } = new();
    public ObservableCollection<ActivityRow> RecentActivity { get; } = new();

    public int MaintenanceLateCount { get; }
    public int MaintenanceSoonCount { get; }

    private bool _isCustomizing;
    public bool IsCustomizing { get => _isCustomizing; set => SetField(ref _isCustomizing, value); }

    private bool _showActivity = true;
    public bool ShowActivity
    {
        get => _showActivity;
        set { if (SetField(ref _showActivity, value) && _loaded) SavePrefs(); }
    }

    public ICommand ToggleCustomizeCommand { get; }

    public DashboardViewModel(AppDataStore store)
    {
        _store = store;
        ToggleCustomizeCommand = new RelayCommand(() => IsCustomizing = !IsCustomizing);

        var res = Application.Current.Resources;
        Brush Bg(string key) => (Brush)res[key];

        var totalRevenus = _store.Revenus.Sum(r => r.Montant);
        var totalDepenses = _store.Depenses.Sum(d => d.Montant);

        Widgets.Add(new DashboardWidget("revenus", "Revenus", totalRevenus.ToString("C", Fr), Bg("AccentSoftBrush")));
        Widgets.Add(new DashboardWidget("depenses", "Dépenses", totalDepenses.ToString("C", Fr), Bg("DangerSoftBrush")));
        Widgets.Add(new DashboardWidget("profit", "Profit", (totalRevenus - totalDepenses).ToString("C", Fr), Bg("InfoSoftBrush")));
        Widgets.Add(new DashboardWidget("clients", "Clients", _store.Clients.Count.ToString(), Bg("PurpleSoftBrush")));
        Widgets.Add(new DashboardWidget("vehicules", "Véhicules", _store.Vehicules.Count.ToString(), Bg("WarningSoftBrush")));
        Widgets.Add(new DashboardWidget("articles", "Articles en inventaire", _store.Inventaire.Count.ToString(), Bg("AccentSoftBrush")));

        MaintenanceLateCount = _store.Maintenances.Count(m => m.GetStatus() == MaintenanceStatus.Late);
        MaintenanceSoonCount = _store.Maintenances.Count(m => m.GetStatus() == MaintenanceStatus.Soon);

        foreach (var i in _store.Inventaire.Where(i => i.IsLowStock))
            LowStockItems.Add(i);

        var recent = _store.Revenus
            .Select(r => new ActivityRow { Date = r.Date, Type = "Revenu", Categorie = r.Categorie, Montant = r.Montant, Description = r.Description })
            .Concat(_store.Depenses.Select(d => new ActivityRow { Date = d.Date, Type = "Dépense", Categorie = d.Categorie, Montant = -d.Montant, Description = d.Description }))
            .OrderByDescending(a => a.Date)
            .Take(8);
        foreach (var row in recent) RecentActivity.Add(row);

        ApplyPrefs();
        foreach (var w in Widgets) w.VisibilityChanged += SavePrefs;
        _loaded = true;
    }

    private void ApplyPrefs()
    {
        var prefs = PreferencesService.Load();
        _showActivity = prefs.DashboardShowActivity;
        OnPropertyChanged(nameof(ShowActivity));

        if (prefs.DashboardWidgets.Count == 0)
        {
            // First run: default to the first four widgets (Revenus/Dépenses/
            // Profit/Clients), matching the website's original 4-card layout.
            foreach (var w in Widgets.Skip(4)) w.IsVisible = false;
            return;
        }

        foreach (var w in Widgets)
        {
            var saved = prefs.DashboardWidgets.FirstOrDefault(p => p.Id == w.Id);
            if (saved != null) w.IsVisible = saved.Visible;
        }
    }

    private void SavePrefs()
    {
        var prefs = PreferencesService.Load();
        prefs.DashboardWidgets = Widgets.Select(w => new DashboardWidgetPref { Id = w.Id, Visible = w.IsVisible }).ToList();
        prefs.DashboardShowActivity = ShowActivity;
        PreferencesService.Save(prefs);
    }
}
