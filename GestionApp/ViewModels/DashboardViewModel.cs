using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using GestionApp.Data;
using GestionApp.Models;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

public class ActivityRow
{
    public string Date { get; set; } = "";
    public string Type { get; set; } = "";
    public string Categorie { get; set; } = "";
    public decimal Montant { get; set; }
    public string Description { get; set; } = "";
}

public class DashboardViewModel : ViewModelBase
{
    private readonly AppDataStore _store;
    private static readonly CultureInfo Fr = new("fr-CA");

    public string CompanyName => _store.Settings.CompanyName;

    public string TotalRevenusText { get; }
    public string TotalDepensesText { get; }
    public string ProfitText { get; }
    public int ClientCount { get; }
    public int VehiculeCount { get; }
    public int MaintenanceLateCount { get; }
    public int MaintenanceSoonCount { get; }

    public ObservableCollection<InventaireItem> LowStockItems { get; } = new();
    public ObservableCollection<ActivityRow> RecentActivity { get; } = new();

    public DashboardViewModel(AppDataStore store)
    {
        _store = store;

        var totalRevenus = _store.Revenus.Sum(r => r.Montant);
        var totalDepenses = _store.Depenses.Sum(d => d.Montant);
        TotalRevenusText = totalRevenus.ToString("C", Fr);
        TotalDepensesText = totalDepenses.ToString("C", Fr);
        ProfitText = (totalRevenus - totalDepenses).ToString("C", Fr);

        ClientCount = _store.Clients.Count;
        VehiculeCount = _store.Vehicules.Count;
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
    }
}
