using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GestionApp.Data;
using GestionApp.Dialogs;
using GestionApp.Models;
using GestionApp.Mvvm;

namespace GestionApp.ViewModels;

public class VehiculesInfoViewModel : ViewModelBase
{
    private readonly AppDataStore _store;
    private readonly Action<string> _setStatus;

    public ObservableCollection<VehiculeCard> Cards { get; } = new();

    private VehiculeCard? _selectedCard;
    public VehiculeCard? SelectedCard
    {
        get => _selectedCard;
        set { if (SetField(ref _selectedCard, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public VehiculesInfoViewModel(AppDataStore store, Action<string> setStatus)
    {
        _store = store;
        _setStatus = setStatus;
        Rebuild();

        AddCommand = new AsyncRelayCommand(AddAsync);
        EditCommand = new AsyncRelayCommand(EditAsync, () => SelectedCard != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedCard != null);
    }

    private void Rebuild()
    {
        Cards.Clear();
        foreach (var v in _store.Vehicules)
        {
            var statuses = _store.Maintenances.Where(m => m.VehiculeId == v.Id).Select(m => m.GetStatus()).ToList();
            Cards.Add(new VehiculeCard(
                v,
                statuses.Count(s => s == MaintenanceStatus.Late),
                statuses.Count(s => s == MaintenanceStatus.Soon),
                statuses.Count(s => s == MaintenanceStatus.Ok)));
        }
    }

    private async Task AddAsync()
    {
        var dlg = new VehiculeDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var ok = await _store.InsertAsync(_store.Vehicules, "vehicules", dlg.Result);
        Rebuild();
        _setStatus(ok ? "Véhicule ajouté." : "Échec de l'ajout : " + _store.LastError);
    }

    private async Task EditAsync()
    {
        if (SelectedCard == null) return;
        var previous = SelectedCard.Vehicule;
        var dlg = new VehiculeDialog(previous) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var ok = await _store.UpdateAsync(_store.Vehicules, "vehicules", previous, dlg.Result);
        if (ok) { _store.RelinkTransactionNames(); _store.RelinkMaintenanceNames(); }
        Rebuild();
        _setStatus(ok ? "Véhicule modifié." : "Échec de la modification : " + _store.LastError);
    }

    private async Task DeleteAsync()
    {
        if (SelectedCard == null) return;
        var v = SelectedCard.Vehicule;
        if (MessageBox.Show(Application.Current.MainWindow,
                $"Supprimer le véhicule « {v.Name} » et son historique de maintenance ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var ok = await _store.DeleteAsync(_store.Vehicules, "vehicules", v);
        if (ok)
        {
            // The DB cascades maintenances on vehicule delete; mirror that locally.
            foreach (var m in _store.Maintenances.Where(m => m.VehiculeId == v.Id).ToList())
                _store.Maintenances.Remove(m);
        }
        Rebuild();
        _setStatus(ok ? "Véhicule supprimé." : "Échec de la suppression : " + _store.LastError);
    }
}
