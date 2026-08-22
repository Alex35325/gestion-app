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

public class InventaireViewModel : ViewModelBase
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

    private InventaireItem? _selectedItem;
    public InventaireItem? SelectedItem
    {
        get => _selectedItem;
        set { if (SetField(ref _selectedItem, value)) CommandManager.InvalidateRequerySuggested(); }
    }

    public int LowStockCount => _store.Inventaire.Count(i => i.IsLowStock);

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public InventaireViewModel(AppDataStore store, Action<string> setStatus)
    {
        _store = store;
        _setStatus = setStatus;

        Items = CollectionViewSource.GetDefaultView(_store.Inventaire);
        Items.Filter = o => o is InventaireItem i && Matches(i);

        AddCommand = new AsyncRelayCommand(AddAsync);
        EditCommand = new AsyncRelayCommand(EditAsync, () => SelectedItem != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedItem != null);
    }

    private bool Matches(InventaireItem i)
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var q = SearchText.Trim();
        return Has(i.Name, q) || Has(i.Category, q) || Has(i.Notes, q);
    }

    private static bool Has(string? s, string q) => !string.IsNullOrEmpty(s) && s.Contains(q, StringComparison.OrdinalIgnoreCase);

    private async Task AddAsync()
    {
        var dlg = new InventaireDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var ok = await _store.InsertAsync(_store.Inventaire, "inventaire", dlg.Result);
        OnPropertyChanged(nameof(LowStockCount));
        _setStatus(ok ? "Article ajouté." : "Échec de l'ajout : " + _store.LastError);
    }

    private async Task EditAsync()
    {
        if (SelectedItem == null) return;
        var previous = SelectedItem;
        var dlg = new InventaireDialog(previous) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() != true) return;
        var ok = await _store.UpdateAsync(_store.Inventaire, "inventaire", previous, dlg.Result);
        if (ok) _store.RelinkTransactionNames();
        OnPropertyChanged(nameof(LowStockCount));
        _setStatus(ok ? "Article modifié." : "Échec de la modification : " + _store.LastError);
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        if (MessageBox.Show(Application.Current.MainWindow, $"Supprimer « {SelectedItem.Name} » ? Cette action est irréversible.",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var ok = await _store.DeleteAsync(_store.Inventaire, "inventaire", SelectedItem);
        OnPropertyChanged(nameof(LowStockCount));
        _setStatus(ok ? "Article supprimé." : "Échec de la suppression : " + _store.LastError);
    }
}
