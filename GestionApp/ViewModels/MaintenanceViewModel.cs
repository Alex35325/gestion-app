using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GestionApp.Data;
using GestionApp.Dialogs;
using GestionApp.Models;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

public class MaintenanceViewModel : ViewModelBase
{
    private readonly AppDataStore _store;
    private readonly Action<string> _setStatus;

    public ICollectionView Items { get; }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { if (SetField(ref _searchText, value)) Items.Refresh(); }
    }

    private Maintenance? _selectedItem;
    public Maintenance? SelectedItem
    {
        get => _selectedItem;
        set { if (SetField(ref _selectedItem, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    public int LateCount => _store.Maintenances.Count(m => m.GetStatus() == MaintenanceStatus.Late);
    public int SoonCount => _store.Maintenances.Count(m => m.GetStatus() == MaintenanceStatus.Soon);
    public int OkCount => _store.Maintenances.Count(m => m.GetStatus() == MaintenanceStatus.Ok);

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public MaintenanceViewModel(AppDataStore store, Action<string> setStatus)
    {
        _store = store;
        _setStatus = setStatus;

        Items = CollectionViewSource.GetDefaultView(_store.Maintenances);
        Items.Filter = o => o is Maintenance m && Matches(m);

        AddCommand = new AsyncRelayCommand(AddAsync);
        EditCommand = new AsyncRelayCommand(EditAsync, () => SelectedItem != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem != null);
    }

    private bool Matches(Maintenance m)
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var q = SearchText.Trim();
        return Has(m.VehiculeName, q) || Has(m.Type, q) || Has(m.Notes, q);
    }

    private static bool Has(string? s, string q) => !string.IsNullOrEmpty(s) && s.Contains(q, StringComparison.OrdinalIgnoreCase);

    private void RaiseCountsChanged()
    {
        OnPropertyChanged(nameof(LateCount));
        OnPropertyChanged(nameof(SoonCount));
        OnPropertyChanged(nameof(OkCount));
    }

    private async Task AddAsync()
    {
        if (_store.Vehicules.Count == 0)
        {
            _setStatus("Ajoutez d'abord un véhicule.");
            return;
        }
        var dlg = new MaintenanceDialog(_store.Vehicules.ToList()) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var result = dlg.Result;
        result.VehiculeName = _store.VehiculeName(result.VehiculeId);
        var ok = await _store.InsertAsync(_store.Maintenances, "maintenances", result);
        RaiseCountsChanged();
        _setStatus(ok ? "Entretien ajouté." : "Échec de l'ajout : " + _store.LastError);
    }

    private async Task EditAsync()
    {
        if (SelectedItem == null) return;
        var previous = SelectedItem;
        var dlg = new MaintenanceDialog(_store.Vehicules.ToList(), previous) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var updated = dlg.Result;
        updated.VehiculeName = _store.VehiculeName(updated.VehiculeId);
        var ok = await _store.UpdateAsync(_store.Maintenances, "maintenances", previous, updated);
        RaiseCountsChanged();
        _setStatus(ok ? "Entretien modifié." : "Échec de la modification : " + _store.LastError);
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        if (MessageBox.Show(Application.Current.MainWindow, "Supprimer cet entretien ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var ok = await _store.DeleteAsync(_store.Maintenances, "maintenances", SelectedItem);
        RaiseCountsChanged();
        _setStatus(ok ? "Entretien supprimé." : "Échec de la suppression : " + _store.LastError);
    }
}
