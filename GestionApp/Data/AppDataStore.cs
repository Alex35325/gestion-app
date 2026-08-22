using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.Data;

/// <summary>
/// Single in-memory copy of everything, backed by Supabase — the desktop
/// equivalent of gestion-app.html's `state` object plus its save/delete
/// functions. Every screen's ViewModel reads these same collections, so an
/// edit made from the Clients tab is immediately visible in Rentabilité,
/// the dashboard, etc. without a manual refresh.
///
/// Each table loads independently (one try/catch per table) so a table that
/// doesn't exist yet, or a transient network error, doesn't take down the
/// rest of the app — same resilience pattern as the website.
///
/// Every write is optimistic: the collection is mutated immediately (so the
/// UI feels instant), then the Supabase call runs; on failure the mutation
/// is rolled back and the error message is surfaced via LastError.
/// </summary>
public class AppDataStore
{
    private readonly SupabaseService _sb = new();

    public ObservableCollection<Client> Clients { get; } = new();
    public ObservableCollection<Revenu> Revenus { get; } = new();
    public ObservableCollection<Depense> Depenses { get; } = new();
    public ObservableCollection<Vehicule> Vehicules { get; } = new();
    public ObservableCollection<InventaireItem> Inventaire { get; } = new();
    public ObservableCollection<Maintenance> Maintenances { get; } = new();
    public AppSettings Settings { get; private set; } = new();

    /// <summary>Raised after any load or successful/failed write, so ViewModels that
    /// derive computed values (Dashboard, Rentabilité, maintenance counts) can refresh.</summary>
    public event Action? Changed;

    public string? LastError { get; private set; }

    public async Task LoadAllAsync()
    {
        await LoadTableAsync(Clients, "clients", c => c.Name);
        RelinkTransactionNames();

        await LoadTableAsync(Revenus, "revenus", r => r.Date, descending: true);
        await LoadTableAsync(Depenses, "depenses", d => d.Date, descending: true);
        await LoadTableAsync(Vehicules, "vehicules", v => v.Name);
        await LoadTableAsync(Inventaire, "inventaire", i => i.Name);
        await LoadTableAsync(Maintenances, "maintenances", m => m.NextDueDate ?? "");

        RelinkTransactionNames();
        RelinkMaintenanceNames();

        try
        {
            var rows = await _sb.GetAllAsync<AppSettings>("settings", "select=*");
            if (rows.Count > 0) Settings = rows[0];
        }
        catch (Exception ex)
        {
            LastError = "Paramètres : " + ex.Message;
        }

        Changed?.Invoke();
    }

    private async Task LoadTableAsync<T>(ObservableCollection<T> target, string table, Func<T, IComparable> sortKey, bool descending = false)
    {
        try
        {
            var rows = await _sb.GetAllAsync<T>(table);
            var sorted = descending ? rows.OrderByDescending(sortKey) : rows.OrderBy(sortKey);
            target.Clear();
            foreach (var row in sorted) target.Add(row);
        }
        catch (Exception ex)
        {
            LastError = $"{table} : {ex.Message}";
        }
    }

    public string ClientName(string? id) => string.IsNullOrEmpty(id) ? "" : Clients.FirstOrDefault(c => c.Id == id)?.Name ?? "";
    public string VehiculeName(string? id) => string.IsNullOrEmpty(id) ? "" : Vehicules.FirstOrDefault(v => v.Id == id)?.Name ?? "";
    public string ProduitName(string? id) => string.IsNullOrEmpty(id) ? "" : Inventaire.FirstOrDefault(i => i.Id == id)?.Name ?? "";

    /// <summary>Recomputes the display-only ClientName/VehiculeName/ProduitName on every
    /// revenu/depense — needed after (re)loading clients/vehicules/inventaire, or after
    /// editing one of those and changing its name.</summary>
    public void RelinkTransactionNames()
    {
        foreach (var r in Revenus) { r.ClientName = ClientName(r.ClientId); r.VehiculeName = VehiculeName(r.VehiculeId); r.ProduitName = ProduitName(r.ProduitId); }
        foreach (var d in Depenses) { d.ClientName = ClientName(d.ClientId); d.VehiculeName = VehiculeName(d.VehiculeId); d.ProduitName = ProduitName(d.ProduitId); }
    }

    public void RelinkMaintenanceNames()
    {
        foreach (var m in Maintenances) m.VehiculeName = VehiculeName(m.VehiculeId);
    }

    // ---------------------------------------------------------------
    // Generic optimistic CRUD — one implementation shared by every
    // entity instead of a copy per table (that duplication is what made
    // the first MVP version hard to extend safely).
    // ---------------------------------------------------------------

    public async Task<bool> InsertAsync<T>(ObservableCollection<T> collection, string table, T item) where T : IEntity
    {
        collection.Add(item);
        Changed?.Invoke();
        try
        {
            await _sb.InsertAsync(table, item);
            return true;
        }
        catch (Exception ex)
        {
            collection.Remove(item);
            LastError = ex.Message;
            Changed?.Invoke();
            return false;
        }
    }

    public async Task<bool> UpdateAsync<T>(ObservableCollection<T> collection, string table, T previous, T updated) where T : IEntity
    {
        var index = collection.IndexOf(previous);
        if (index < 0) return false;
        collection[index] = updated;
        Changed?.Invoke();
        try
        {
            await _sb.UpdateAsync(table, updated.Id, updated);
            return true;
        }
        catch (Exception ex)
        {
            collection[index] = previous;
            LastError = ex.Message;
            Changed?.Invoke();
            return false;
        }
    }

    public async Task<bool> DeleteAsync<T>(ObservableCollection<T> collection, string table, T item) where T : IEntity
    {
        var index = collection.IndexOf(item);
        if (index < 0) return false;
        collection.RemoveAt(index);
        Changed?.Invoke();
        try
        {
            await _sb.DeleteAsync(table, item.Id);
            return true;
        }
        catch (Exception ex)
        {
            collection.Insert(index, item);
            LastError = ex.Message;
            Changed?.Invoke();
            return false;
        }
    }

    public async Task<bool> SaveSettingsAsync(AppSettings updated)
    {
        var previous = Settings;
        Settings = updated;
        Changed?.Invoke();
        try
        {
            await _sb.UpdateAsync("settings", "1", updated);
            return true;
        }
        catch (Exception ex)
        {
            Settings = previous;
            LastError = ex.Message;
            Changed?.Invoke();
            return false;
        }
    }
}
